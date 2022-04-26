using System;
using System.Collections.Generic;
using System.Threading;
using RankWidthApproximate.Data.Graph;
using RankWidthApproximate.LocalSearch.CutFunction;
using RankWidthApproximate.LocalSearch.Operators;

namespace RankWidthApproximate.LocalSearch
{
    public class SASearch
    {
        public List<(int probFactor, ISearchOperator op)> Operators { get; } = new();

        public float InitialTemperature { get; set; } = 5;
        public int   ThresholdDelta     { get; set; } = -1;

        public SearchContext Context { get; private set; }

        public event Action<SearchContext>        BetterWidthFound;
        public event Action<SearchContext>        ImprovementFound;
        public event Action<SearchContext, float> TemperatureUpdated;
        public event Action<SearchContext>        SearchFinished;

        private volatile bool   _stopSearch;
        private          Thread _searchThread;

        private bool _adaptiveCooling;

        public void BeginSearch(Graph graph, ICutFunction cutFunction, int seed = -1, int initialSolutionSeed = -1,
            bool adaptiveCooling = true)
        {
            if (Operators.Count == 0)
                throw new Exception("No operators specified");

            Context = new SearchContext(graph, cutFunction, ThresholdDelta, seed, initialSolutionSeed);

            _adaptiveCooling = adaptiveCooling;
            _stopSearch      = false;
            _searchThread    = new Thread(SearchMain);
            _searchThread.Start();
        }

        /// <summary>
        /// Restarts the search with the previous decomposition as initial solution
        /// </summary>
        public void RestartSearch()
        {
            if (Context == null)
                throw new Exception("Can only restart when initial search was done!");

            _stopSearch   = false;
            _searchThread = new Thread(SearchMain);
            _searchThread.Start();
        }

        /// <summary>
        /// Stops the search
        /// </summary>
        public void StopSearch()
        {
            _stopSearch = true;
            WaitForCompletion();
        }

        /// <summary>
        /// Waits until the search has ended
        /// </summary>
        public void WaitForCompletion()
        {
            _searchThread.Join();
        }

        /// <summary>
        /// Main function of the search thread
        /// </summary>
        private void SearchMain()
        {
            Context.BestWidth              = Context.Width;
            Context.BestDecompositionScore = Context.Score;
            Context.BestDecomposition      = Context.Decomposition.ToDotGraph(Context);
            BetterWidthFound?.Invoke(Context); //report initial score

            var opAccFactors = new int[Operators.Count];
            opAccFactors[0] = Operators[0].probFactor;
            for (int i = 1; i < Operators.Count; i++)
                opAccFactors[i] = opAccFactors[i - 1] + Operators[i].probFactor;

            float saT2 = InitialTemperature;
            float saT  = saT2;
            float saA  = 0.95f;
            int   saQ  = 25600;

            int pickCount  = 0;
            int totalCount = 0;

            while (!_stopSearch)
            {
                int curQ = saQ;
                while (curQ > 0 && !_stopSearch)
                {
                    int prob = Context.Random.Next(opAccFactors[^1]);

                    int op = 0;
                    while (prob >= opAccFactors[op])
                        op++;

                    Context.StoreScore();

                    Operators[op].op.Perform(Context);

                    bool pick = Context.Score <= Context.OldScore && Context.Score >= 0;
                    if (!pick && Context.Score >= 0)
                    {
                        pick = Context.Random.NextDouble() < Math.Exp((Context.OldScore - Context.Score) / saT);
                        totalCount++;
                        if (pick)
                            pickCount++;
                    }

                    if (!pick)
                    {
                        Operators[op].op.Undo(Context);
                        Context.RestoreScore();
                    }
                    else
                    {
                        if (Context.Width < Context.BestWidth)
                        {
                            Context.BestWidth              = Context.Width;
                            Context.BestDecompositionScore = Context.Score;
                            Context.BestDecomposition      = Context.Decomposition.ToDotGraph(Context);
                            BetterWidthFound?.Invoke(Context);
                        }

                        if (Context.Score < Context.BestScore)
                        {
                            Context.BestScore = Context.Score;
                            ImprovementFound?.Invoke(Context);
                        }
                    }

                    curQ--;
                }

                saT2 *= saA;
                if (saT2 < 0.05f)
                    break;

                if (_adaptiveCooling)
                    saT = saT2 * (1 + (Context.Score - Context.BestScore) / (float)Context.Score);
                else
                    saT = saT2;

                TemperatureUpdated?.Invoke(Context, saT);
            }

            if (Context.Score < Context.BestDecompositionScore && Context.Width <= Context.BestWidth)
            {
                //if the decomposition was still slightly improved after the last width decrease
                //update the best decomposition
                Context.BestWidth              = Context.Width;
                Context.BestDecompositionScore = Context.Score;
                Context.BestDecomposition      = Context.Decomposition.ToDotGraph(Context);
            }

            SearchFinished?.Invoke(Context);
        }
    }
}