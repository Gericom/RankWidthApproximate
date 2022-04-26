using System;
using System.Collections.Generic;
using System.IO;
using RankWidthApproximate.Data.Graph;

namespace RankWidthApproximate.Data
{
    public static class DimacsParser
    {
        /// <summary>
        /// Parses an undirected dimacs graph
        /// </summary>
        /// <param name="path">The path to the undirected dimacs graph file</param>
        /// <returns>An adjacency matrix graph of the parsed graph</returns>
        public static AdjMtxGraph ParseUndirected(string path)
        {
            AdjMtxGraph result = null;
            using (var reader = new StreamReader(path))
            {
                string line;
                bool   gotHeader = false;
                int    nodeCount = 0;
                int    edgeCount = 0;
                var    labelToId = new Dictionary<string, int>();
                int    curId     = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) //skip empty lines
                        continue;

                    var parts = line.Trim().Split();

                    if (parts[0] == "c") //comment line
                        continue;

                    if (parts[0] == "x") //skip graph param
                        continue;

                    if (parts[0] == "n") //skip vertex param
                        continue;

                    if (!gotHeader)
                    {
                        if (parts[0] != "p")
                            throw new InvalidDataException("Expected 'p' command");

                        nodeCount = int.Parse(parts[2]);
                        edgeCount = int.Parse(parts[3]);

                        result = new AdjMtxGraph(nodeCount);

                        gotHeader = true;
                        continue;
                    }

                    if (edgeCount <= 0)
                        throw new InvalidDataException("Invalid number of edges specified");

                    int i = 0;
                    //edges can be directly specified with two vertex labels, or they can be prefixed by an 'e'
                    if (parts.Length >= 3)
                    {
                        if (parts[0] != "e")
                            throw new InvalidDataException("Expected 'e' command");
                        i++;
                    }

                    //convert the vertex labels to an id

                    if (!labelToId.ContainsKey(parts[i]))
                    {
                        labelToId.Add(parts[i], curId);
                        result.VtxLabels[curId] = parts[i];
                        curId++;
                    }

                    if (!labelToId.ContainsKey(parts[i + 1]))
                    {
                        labelToId.Add(parts[i + 1], curId);
                        result.VtxLabels[curId] = parts[i + 1];
                        curId++;
                    }

                    int a = labelToId[parts[i]];
                    int b = labelToId[parts[i + 1]];

                    if (a < 0 || a >= nodeCount || b < 0 || b >= nodeCount)
                        throw new InvalidDataException("Invalid number of vertices specified");

                    result.AdjacencyMtx[a, b] = true;
                    result.AdjacencyMtx[b, a] = true;

                    edgeCount--;
                }

                if (edgeCount != 0)
                    throw new InvalidDataException("Header edge count invalid");

                // if (curId != nodeCount)
                // throw new InvalidDataException("Header vertex count invalid");

                if (curId != nodeCount)
                {
                    //for some reason there are lonely nodes
                    for (int i = curId; i < nodeCount; i++)
                        result.VtxLabels[i] = "unk_" + i;
                }
            }

            return result;
        }

        /// <summary>
        /// Parses a directed dimacs graph
        /// </summary>
        /// <param name="path">The path to the directed dimacs graph file</param>
        /// <returns>A GF4 directed graph of the parsed graph</returns>
        public static GF4DirectedGraph ParseDirected(string path)
        {
            GF4DirectedGraph result = null;

            void addForwardArc(int a, int b)
            {
                result.AdjacencyMtx[a, b] = result.AdjacencyMtx[a, b] switch
                {
                    GF4.Zero        => GF4.Alpha,
                    GF4.One         => GF4.One,
                    GF4.Alpha       => GF4.Alpha,
                    GF4.AlphaSquare => GF4.One,
                    _               => throw new ArgumentOutOfRangeException()
                };
            }

            void addBackwardArc(int a, int b)
            {
                result.AdjacencyMtx[a, b] = result.AdjacencyMtx[a, b] switch
                {
                    GF4.Zero        => GF4.AlphaSquare,
                    GF4.One         => GF4.One,
                    GF4.Alpha       => GF4.One,
                    GF4.AlphaSquare => GF4.AlphaSquare,
                    _               => throw new ArgumentOutOfRangeException()
                };
            }

            using (var reader = new StreamReader(path))
            {
                string line;
                bool   gotHeader = false;
                int    nodeCount = 0;
                int    arcCount  = 0;
                var    labelToId = new Dictionary<string, int>();
                int    curId     = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) //skip empty lines
                        continue;

                    var parts = line.Trim().Split();

                    if (parts[0] == "c") //comment line
                        continue;

                    if (parts[0] == "x") //skip graph param
                        continue;

                    if (parts[0] == "n") //skip vertex param
                        continue;

                    if (!gotHeader)
                    {
                        if (parts[0] != "p")
                            throw new InvalidDataException("Expected 'p' command");

                        nodeCount = int.Parse(parts[2]);
                        arcCount  = int.Parse(parts[3]);

                        result = new GF4DirectedGraph(nodeCount);

                        gotHeader = true;
                        continue;
                    }

                    if (arcCount <= 0)
                        throw new InvalidDataException("Invalid number of edges specified");

                    int i = 0;
                    //arcs can be directly specified with two vertex labels, or they can be prefixed by an 'a'
                    if (parts.Length > 2)
                    {
                        if (parts[0] != "a")
                            throw new InvalidDataException("Expected 'a' command");
                        i++;
                    }

                    //convert the vertex labels to an id

                    if (!labelToId.ContainsKey(parts[i]))
                    {
                        labelToId.Add(parts[i], curId);
                        result.VtxLabels[curId] = parts[i];
                        curId++;
                    }

                    if (!labelToId.ContainsKey(parts[i + 1]))
                    {
                        labelToId.Add(parts[i + 1], curId);
                        result.VtxLabels[curId] = parts[i + 1];
                        curId++;
                    }

                    int a = labelToId[parts[i]];
                    int b = labelToId[parts[i + 1]];

                    if (a < 0 || a >= nodeCount || b < 0 || b >= nodeCount)
                        throw new InvalidDataException("Invalid number of vertices specified");

                    addForwardArc(a, b);
                    addBackwardArc(b, a);

                    arcCount--;
                }

                if (arcCount != 0)
                    throw new InvalidDataException("Header arc count invalid");

                // if (curId != nodeCount)
                //     throw new InvalidDataException("Header vertex count invalid");

                if (curId != nodeCount)
                {
                    //for some reason there are lonely nodes
                    for (int i = curId; i < nodeCount; i++)
                        result.VtxLabels[i] = "unk_" + i;
                }
            }

            return result;
        }
    }
}