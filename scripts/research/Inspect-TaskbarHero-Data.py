"""Read-only extraction of TaskbarHero's shipped TextAssets and localization.

Requires UnityPy 1.24.2; writes only to the explicitly supplied output directories.
"""
import argparse
import collections
import csv
from datetime import datetime, timezone
import hashlib
import io
import json
from pathlib import Path
import UnityPy

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--installation", type=Path, required=True, help="TaskbarHero installation directory")
parser.add_argument("--output", type=Path, required=True, help="Private extraction output directory")
parser.add_argument("--evidence", type=Path, help="Optional portable summary output directory")
args = parser.parse_args()
installation = args.installation.resolve(strict=True)
for destination in (args.output, args.evidence):
    if destination and (destination.resolve() == installation or installation in destination.resolve().parents):
        parser.error("Output must be outside the installed game")
ROOT = args.installation / "TaskBarHero_Data"
OUT = args.output
for sub in ("tables", "localization"):
    (OUT / sub).mkdir(parents=True, exist_ok=True)


def write_json(path, data):
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")


manifest = {"scope": "shipped static resources; no game execution", "tables": [], "localization": []}
env = UnityPy.load(str(ROOT / "sharedassets0.assets"))
for obj in env.objects:
    if obj.type.name != "TextAsset":
        continue
    data = obj.read()
    source = data.m_Script
    if isinstance(source, bytes):
        source = source.decode("utf-8-sig")
    else:
        source = source.lstrip("\ufeff")
    target = OUT / "tables" / (data.m_Name + ".csv")
    target.write_text(source, encoding="utf-8", newline="")
    reader = csv.DictReader(io.StringIO(source, newline=""))
    rows = list(reader)
    fields = reader.fieldnames or []
    profile = {}
    for field in fields:
        values = [r.get(field) for r in rows if r.get(field) not in (None, "")]
        counts = collections.Counter(values)
        numbers = []
        for value in values:
            try:
                numbers.append(float(value))
            except (ValueError, TypeError):
                pass
        profile[field] = {"filled": len(values), "distinct": len(counts), "samples": counts.most_common(8)}
        if numbers and len(numbers) == len(values):
            profile[field]["min"] = min(numbers)
            profile[field]["max"] = max(numbers)
    first_field_counts = collections.Counter(r[fields[0]] for r in rows)
    entry = {"name": data.m_Name, "source": "TaskBarHero_Data/sharedassets0.assets", "pathID": obj.path_id,
             "rowCount": len(rows), "fields": fields, "fieldProfile": profile,
             "first_field_unique_count": len(first_field_counts),
             "first_field_duplicates": {k: v for k, v in first_field_counts.items() if v > 1},
             "sha256_utf8_without_bom": hashlib.sha256(source.encode("utf-8")).hexdigest(),
             "malformed_rows": [i + 2 for i, r in enumerate(rows) if None in r or any(v is None for v in r.values())]}
    manifest["tables"].append(entry)

bundles = ROOT / "StreamingAssets/aa/StandaloneWindows64"
shared = {}
shared_name = "localization-assets-shared_assets_all.bundle"
for obj in UnityPy.load(str(bundles / shared_name)).objects:
    if obj.type.name != "MonoBehaviour":
        continue
    tree = obj.read_typetree()
    if "m_Entries" not in tree:
        continue
    shared[obj.path_id] = tree
    write_json(OUT / "localization" / (tree["m_TableCollectionName"] + ".shared.json"),
               {"source": shared_name, "pathID": obj.path_id, "data": tree})

status_labels = {}
status_keys = {"Status", "Status_RemainedSkillPoint", "UI_Status_LevelUp", "UI_Status_Refund", "UI_Status_Equip", "UI_Status_Unequip", "UI_Status_RequiredPoints", "UI_Status_DetailedStats", "NormalAttackDPS"}
for token, locale in (("english", "en-US"), ("portuguese", "pt-BR")):
    bundle = next(bundles.glob("*" + token + "*.bundle"))
    combined = {}
    status_labels[locale] = []
    for obj in UnityPy.load(str(bundle)).objects:
        if obj.type.name != "MonoBehaviour":
            continue
        tree = obj.read_typetree()
        if "m_TableData" not in tree:
            continue
        shared_id = tree["m_SharedData"]["m_PathID"]
        common = shared[shared_id]
        ids = {r["m_Id"]: r["m_Key"] for r in common["m_Entries"]}
        mapping = [{"id": r["m_Id"], "key": ids.get(r["m_Id"]), "text": r["m_Localized"]} for r in tree["m_TableData"]]
        for r in mapping:
            if r["key"]:
                combined[r["key"]] = r["text"]
        info = {"table": tree["m_Name"], "source": bundle.name, "pathID": obj.path_id,
                "shared_source": shared_name, "shared_pathID": shared_id, "rowCount": len(mapping),
                "missing_shared_keys": sum(r["key"] is None for r in mapping)}
        status_labels[locale].extend({"key": r["key"], "text": r["text"], "entry_id": r["id"], "source": bundle.name, "pathID": obj.path_id} for r in mapping if r["key"] in status_keys)
        write_json(OUT / "localization" / (tree["m_Name"] + ".json"), {**info, "entries": mapping})
        manifest["localization"].append(info)
    write_json(OUT / "localization" / (locale + ".keys.json"), combined)
write_json(OUT / "summary-systems.json", manifest)
if args.evidence:
    args.evidence.mkdir(parents=True, exist_ok=True)
    portable_tables = []
    for t in manifest["tables"]:
        portable_tables.append({k: t[k] for k in ("name", "source", "pathID", "rowCount", "fields", "sha256_utf8_without_bom", "first_field_unique_count", "malformed_rows")})
        portable_tables[-1]["numeric_ranges"] = {k: {"min": v["min"], "max": v["max"]} for k, v in t["fieldProfile"].items() if "min" in v}
    write_json(args.evidence / "Tabelas_Resumo.json", {"evidence_id": "D01", "extracted": datetime.now(timezone.utc).date().isoformat(), "parser": "UnityPy " + UnityPy.__version__, "scope": manifest["scope"], "table_count": len(portable_tables), "total_rows": sum(t["rowCount"] for t in portable_tables), "tables": portable_tables})
    write_json(args.evidence / "Localizacao_Resumo.json", {"evidence_id": "L01", "scope": "StringTable and ItemTable: en-US and pt-BR; static artifact data", "source_directory": "TaskBarHero_Data/StreamingAssets/aa/StandaloneWindows64", "shared_tables": [{"source": shared_name, "pathID": key, "name": val["m_Name"], "key_count": len(val["m_Entries"])} for key, val in shared.items()], "locale_tables": manifest["localization"], "selected_status_labels": status_labels})
print(json.dumps({"tables": len(manifest["tables"]), "rows": sum(x["rowCount"] for x in manifest["tables"]),
                  "localization": manifest["localization"]}, ensure_ascii=True, indent=2))
