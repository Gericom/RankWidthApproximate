using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using RankWidthApproximate.Data;
using RankWidthApproximate.Data.Graph;
using RankWidthApproximate.LocalSearch;
using RankWidthApproximate.LocalSearch.CutFunction;
using RankWidthApproximate.LocalSearch.Operators;

namespace RankWidthApproximate
{
    internal static class Program
    {
        private enum InputMode
        {
            Undirected,
            Directed,
            DirectedToUndirected
        }

        private enum WidthType
        {
            RankWidth,
            F4RankWidth,
            MmWidth
        }

        static void Main(string[] args)
        {
            var  search              = new SASearch();
            var  mode                = InputMode.Undirected;
            var  widthType           = WidthType.RankWidth;
            int  seed                = -1;
            int  initialSolutionSeed = -1;
            int  timeLimit           = 0;
            bool adaptiveCooling     = false;
            bool verbose             = false;

            if (args.Length == 0 || args.Length == 1 && args[0] == "-h")
            {
                Console.WriteLine("Usage: RankWidthApproximate.exe [options] graph.dgf");
                Console.WriteLine();
                Console.WriteLine("By default the rank-width of an undirected graph is approximated");
                Console.WriteLine();
                Console.WriteLine("-ac       Enables adaptive cooling");
                Console.WriteLine("-d        Approximate F4-rank-width of a directed graph (don't use with -mm)");
                Console.WriteLine("-d2u      Converts a directed input graph to undirected");
                Console.WriteLine("-is seed  Sets the seed for the rng for the initial solution");
                Console.WriteLine("-mm       Approximate the maximum matching-width of an undirected graph");
                Console.WriteLine("-s  seed  Sets the seed for the rng");
                Console.WriteLine("-t  temp  Sets the initial temperature (default 5.0)");
                Console.WriteLine("-td delta Enables the thresholding heuristic with the given delta");
                Console.WriteLine("-tl sec   Stops the search process after the given number of seconds");
                Console.WriteLine("-v        Output more information during the search process");
                return;
            }

            int i;
            for (i = 0; i < args.Length - 1; i++)
            {
                switch (args[i])
                {
                    case "-d":
                        mode = InputMode.Directed;
                        break;
                    case "-d2u":
                        mode = InputMode.DirectedToUndirected;
                        break;
                    case "-mm":
                        widthType = WidthType.MmWidth;
                        break;
                    case "-t":
                        search.InitialTemperature = float.Parse(args[++i], CultureInfo.InvariantCulture);
                        break;
                    case "-s":
                        seed = int.Parse(args[++i]);
                        break;
                    case "-is":
                        initialSolutionSeed = int.Parse(args[++i]);
                        break;
                    case "-td":
                        search.ThresholdDelta = int.Parse(args[++i]);
                        break;
                    case "-tl":
                        timeLimit = int.Parse(args[++i]);
                        break;
                    case "-ac":
                        adaptiveCooling = true;
                        break;
                    case "-v":
                        verbose = true;
                        break;
                    default:
                        Console.WriteLine("Invalid argument specified!");
                        Console.WriteLine("Use -h for help");
                        return;
                }
            }

            if (mode == InputMode.Directed && widthType == WidthType.MmWidth)
            {
                Console.WriteLine("Maximum-matching-width cannot be used with directed graphs!");
                Console.WriteLine("To convert the directed graph to undirected use the -d2u option");
                return;
            }

            if (mode == InputMode.Directed && widthType == WidthType.RankWidth)
                widthType = WidthType.F4RankWidth;

            Graph        graph       = null;
            ICutFunction cutFunction = null;

            switch (mode)
            {
                case InputMode.Undirected:
                    if (widthType == WidthType.MmWidth)
                        graph = new AdjListGraph(DimacsParser.ParseUndirected(args[i]));
                    else
                        graph = DimacsParser.ParseUndirected(args[i]);
                    break;

                case InputMode.Directed:
                    graph = DimacsParser.ParseDirected(args[i]);
                    break;

                case InputMode.DirectedToUndirected:
                    if (widthType == WidthType.MmWidth)
                        graph = new AdjListGraph(DimacsParser.ParseDirected(args[i]).ToUndirected());
                    else
                        graph = DimacsParser.ParseDirected(args[i]).ToUndirected();
                    break;
            }

            switch (widthType)
            {
                case WidthType.RankWidth:
                    cutFunction = new CutRankFunction();
                    Console.WriteLine("Approximating rank-width...");
                    break;
                case WidthType.F4RankWidth:
                    cutFunction = new F4CutRankFunction();
                    Console.WriteLine("Approximating F4-rank-width...");
                    break;
                case WidthType.MmWidth:
                    cutFunction = new MmCutFunction();
                    Console.WriteLine("Approximating maximum-matching-width...");
                    break;
            }

            if (graph.VtxCount < 2)
            {
                Console.WriteLine("Graph has fewer than 2 vertices, width is 0");
                return;
            }

            if (!graph.CheckHasEdges())
            {
                Console.WriteLine("Graph has no edges, width is 0");
                return;
            }

            search.Operators.Add((5000, new LeafSwapOperator()));
            search.Operators.Add((20000, new LocalSwapOperator()));
            search.Operators.Add((25000, new MoveOperator()));

#if LOG_PROGRESS
            var searchProgressLog = new List<(long, int)>();
            var stopwatch         = new Stopwatch();
#endif

            search.BetterWidthFound += context =>
            {
                if (verbose)
                    Console.WriteLine($"Width Improvement: {context.Score}, width: {context.Width}");
#if LOG_PROGRESS
                searchProgressLog.Add((stopwatch.ElapsedMilliseconds, context.Width));
#endif
            };

            if (verbose)
            {
                search.TemperatureUpdated += (context, temperature) =>
                {
                    Console.WriteLine($"Current score: {context.Score}");
                    Console.WriteLine($"Current temperature: {temperature}");
                };

                search.ImprovementFound += context =>
                {
                    Console.WriteLine($"Improvement: {context.Score}, width: {context.Width}");
                };
            }

            if (timeLimit > 0)
            {
                var mainThread = Thread.CurrentThread;
                search.SearchFinished += context => { mainThread.Interrupt(); };
            }

#if LOG_PROGRESS
            stopwatch.Start();
#endif
            search.BeginSearch(graph, cutFunction, seed, initialSolutionSeed, adaptiveCooling);

            Console.CancelKeyPress += (_, a) =>
            {
                a.Cancel = true;
                search.StopSearch();
            };

            if (timeLimit > 0)
            {
                try
                {
                    Thread.Sleep(timeLimit * 1000);
                    search.StopSearch();
                }
                catch (ThreadInterruptedException) { }
            }
            else
                search.WaitForCompletion();

            Console.WriteLine(search.Context.BestDecomposition);
            Console.WriteLine("Search finished with best width: " + search.Context.BestWidth);

#if CACHE_HISTOGRAM
            Console.WriteLine();
            Console.WriteLine("Cache hit histogram:");
            for (int j = 0; j < 20; j++)
                Console.WriteLine($"{j * 5}-{j * 5 + 5}%; {search.Context.CacheHistogram[j]}");
#endif

#if LOG_PROGRESS
            Console.WriteLine();
            Console.WriteLine("Progress log:");
            foreach (var (time, width) in searchProgressLog)
                Console.WriteLine($"{time};{width}");
#endif
        }
    }
}