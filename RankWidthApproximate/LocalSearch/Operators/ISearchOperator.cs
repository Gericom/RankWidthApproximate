namespace RankWidthApproximate.LocalSearch.Operators
{
    public interface ISearchOperator
    {
        public void Perform(SearchContext context);
        public void Undo(SearchContext context);
    }
}