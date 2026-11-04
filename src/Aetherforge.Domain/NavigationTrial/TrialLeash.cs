namespace Aetherforge.Domain.NavigationTrial
{
    // The pilot has no enemy/HP aggregate. This query exercises the future hostile
    // leash metric using the SAME bounded search/area and exact graph costs.
    public sealed class TrialLeash
    {
        public TrialSearch Search { get; }
        public bool? Within => Search.Status==TrialSearchStatus.Pending ? (bool?)null : Search.Status==TrialSearchStatus.Found && Search.Cost<=6000000;
        public TrialLeash(TrialGrid grid,TrialPoint spawn,TrialPoint candidate,TrialRect area,long requestKey)
        { Search=new TrialSearch(grid,spawn,candidate,TrialBranch.North,requestKey,area:area); }
        // A certificate for the WHOLE proposed motion, including the leash boundary.
        public bool Certifies(TrialPoint from,TrialPoint to) => Within==true && Search.Destination.Equals(from) && Search.Cost<=6000000-TrialPoint.Length(from,to);
    }
}
