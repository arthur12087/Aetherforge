"""Read-only Unity UI inventory. Requires UnityPy 1.24.2; outputs analysis only.

Usage: python Inspect-TaskbarHero-Ui.py --game <installed game> --output <new directory>
Does not load the game's executable, inspect saves, or export image/audio assets.
"""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import re

import UnityPy


def write_json(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding="utf-8")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--game", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--evidence", type=Path, help="Optional portable summary directory")
    args = parser.parse_args()
    game = args.game.resolve(strict=True)
    output = args.output.resolve()
    if output == game or game in output.parents:
        parser.error("Output must be outside the installed game")
    if args.evidence and (args.evidence.resolve() == game or game in args.evidence.resolve().parents):
        parser.error("Evidence must be outside the installed game")
    output.mkdir(parents=True, exist_ok=True)
    data = game / "TaskBarHero_Data"
    manifest = []
    for path in sorted(game.rglob("*")):
        if path.is_file():
            with path.open("rb") as stream:
                digest = hashlib.file_digest(stream, "sha256").hexdigest()
            manifest.append({"path": path.relative_to(game).as_posix(), "bytes": path.stat().st_size, "sha256": digest})
    write_json(output / "manifest.json", {"root": str(game), "version": (game / "Version.txt").read_text(), "files": manifest})

    script_env = UnityPy.load(str(data / "globalgamemanagers.assets"))
    scripts = {}
    errors = []
    header_only = 0
    for obj in script_env.objects:
        if obj.type.name == "MonoScript":
            t = obj.read_typetree()
            scripts[obj.path_id] = {k: t.get(k) for k in ("m_ClassName", "m_Namespace", "m_AssemblyName")}
    write_json(output / "monoscripts.json", scripts)
    env = UnityPy.load(str(data / "level0"))
    objects, transforms, components = {}, {}, {}
    type_counts = Counter()
    for obj in env.objects:
        typ = obj.type.name
        type_counts[typ] += 1
        if typ not in ("GameObject", "RectTransform", "Transform", "MonoBehaviour", "Canvas", "CanvasGroup"):
            continue
        try:
            t = obj.read_typetree()
        except Exception as exc:
            errors.append({"path_id": obj.path_id, "type": typ, "error": str(exc)})
            if typ != "MonoBehaviour":
                continue
            # Read the known base header; do not pretend trailing custom data decoded.
            t = obj.read_typetree(check_read=False)
            header_only += 1
        if typ == "GameObject":
            objects[obj.path_id] = {"path_id": obj.path_id, "name": t.get("m_Name"), "active_self": t.get("m_IsActive"), "component_ids": [x["component"]["m_PathID"] for x in t["m_Component"]]}
        elif typ in ("RectTransform", "Transform"):
            transforms[obj.path_id] = {"game_object": t["m_GameObject"]["m_PathID"], "parent": t["m_Father"]["m_PathID"], "children": [x["m_PathID"] for x in t["m_Children"]], "layout": {k: v for k, v in t.items() if k not in ("m_GameObject", "m_Father", "m_Children")}}
        else:
            component = {"path_id": obj.path_id, "type": typ, "data": t, "decode": "base-header-only" if typ == "MonoBehaviour" and len(t) <= 5 else "typetree"}
            if typ == "MonoBehaviour":
                pptr = t.get("m_Script", {})
                component["script"] = scripts.get(pptr.get("m_PathID")) if pptr.get("m_FileID") == 1 else None
                # Base UnityPy parsing may expose only the MonoBehaviour header.
                # Raw UTF-8 candidates are separately marked, never decoded fields.
                raw = obj.get_raw_data()
                component["raw_ascii_candidates"] = [m.group().decode("ascii") for m in re.finditer(rb"[\x20-\x7e]{5,}", raw)]
            components[obj.path_id] = component

    go_transform = {t["game_object"]: tid for tid, t in transforms.items()}
    def full_path(tid, visited=None):
        if not tid or tid not in transforms:
            return ""
        visited = set() if visited is None else visited
        if tid in visited:
            return "[cycle]"
        visited.add(tid)
        t = transforms[tid]
        name = objects.get(t["game_object"], {}).get("name", "?")
        prefix = full_path(t["parent"], visited)
        return f"{prefix}/{name}" if prefix else name

    rows = []
    for goid, go in objects.items():
        tid = go_transform.get(goid)
        transform = transforms.get(tid, {})
        row = {**go, "transform_id": tid, "path": full_path(tid), "parent_transform": transform.get("parent"), "layout": transform.get("layout"), "components": [components[c] for c in go["component_ids"] if c in components]}
        rows.append(row)
    rows.sort(key=lambda r: (r["path"], r["path_id"]))
    write_json(output / "ui-hierarchy.json", rows)
    interesting = re.compile(r"status|statpanel|hero|inventory|skill|portal|setting|offline|formation|attribute|dps|statistic", re.I)
    selected = [row for row in rows if interesting.search(row["path"])]
    write_json(output / "ui-selected.json", selected)
    summary = {"unitypy": UnityPy.__version__, "source": str(data / "level0"), "counts": dict(type_counts), "game_objects_decoded": len(objects), "transforms_decoded": len(transforms), "full_component_decode_errors": len(errors), "monobehaviour_base_header_fallbacks": header_only, "hierarchy_roots": [r["path"] for r in rows if not r["parent_transform"]], "matching_objects": len(selected), "limitations": ["Serialized scene state only; no runtime interaction", "GameObject active_self is not effective visibility", "Raw ASCII candidates are not decoded text or event bindings", "Missing script fields must remain unknown"]}
    write_json(output / "ui-summary.json", summary)
    write_json(output / "ui-errors.json", errors)
    if args.evidence:
        args.evidence.mkdir(parents=True, exist_ok=True)
        portable = [
            {"path_id": r["path_id"], "path": r["path"], "active_self": r["active_self"],
             "layout": r["layout"], "scripts": [c["script"]["m_ClassName"] for c in r["components"] if c.get("script")]}
            for r in rows if re.search(r"^Canvas_Main/Main/(Left|Center|Right)$|^Canvas_Main/Main/(Left|Center|Right)/[^/]+$|^Canvas_Main/Main/Left/UI_Status(/Top Panel(/StatusPanel(/(Left Panel|Right Panel))?)?|/Bottom Panel(/(Skill Panel|Detailed Stats Panel))?|/SkillPoint Panel|/TeamStatus|/TeamStatsButton|/Button_Close|/BG _Decor)$", r["path"])
        ]
        write_json(args.evidence / "UI_Resumo.json", {"evidence_id": "U01", "summary": summary, "nodes": portable})
        write_json(args.evidence / "Instalacao.json", {"root": str(game), "version": (game / "Version.txt").read_text(), "files": manifest})
    print(json.dumps(summary, ensure_ascii=False))


if __name__ == "__main__":
    main()
