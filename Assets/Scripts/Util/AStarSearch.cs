using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class AStarSearch
{
    public static float Heuristic(Tile a, Tile b)
    {
        return Vector2.Distance(a.hex.ScreenPos(), b.hex.ScreenPos());
    }

    public static List<Tile> ShortestPath(Tile start, Tile goal)
    {
        List<Tile> path = new List<Tile>();
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, float> costSoFar = new Dictionary<Tile, float>();

        PriorityQueue<Tile> frontier = new PriorityQueue<Tile>();
        frontier.Enqueue(start, 0);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        while(frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();
            if (current == goal) break;

            if (current.containedWorker == null || current == start)
            {
                foreach (Tile next in current.neighbors)
                {
                    float newCost = costSoFar[current] + 1;
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        float priority = newCost + Heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }
        }

        Tile pathNode = goal;
        while(pathNode != start)
        {
            path.Add(pathNode);
            pathNode = cameFrom[pathNode];
        }

        return path;
    }

    public static List<Tile> FindAllAvailableGoals(Tile start, int movesAvailable)
    {
        List<Tile> availableGoals = new List<Tile>();
        if (movesAvailable == 0) return availableGoals;
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, int> costSoFar = new Dictionary<Tile, int>();

        Queue<Tile> frontier = new Queue<Tile>();
        frontier.Enqueue(start);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();
            if (costSoFar[current] <= movesAvailable)
            {
                if(current != start) availableGoals.Add(current);
                if (current.containedWorker == null || current == start)
                {
                    foreach (Tile next in current.neighbors)
                    {
                        int newCost = costSoFar[current] + 1;
                        if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                        {
                            costSoFar[next] = newCost;
                            frontier.Enqueue(next);
                            cameFrom[next] = current;
                        }
                    }
                }
            }
        }
        return availableGoals;
    }
}

public class PriorityQueue<T>
{
    public List<PrioritizedItem<T>> elements = new List<PrioritizedItem<T>>();

    public int Count { get { return elements.Count; } }

    public void Enqueue(T item, float priority)
    {
        elements.Add(new PrioritizedItem<T>(item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].priority < elements[bestIndex].priority) bestIndex = i;
        }

        T bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}

public class PrioritizedItem<T>
{
    public T item;
    public float priority;
    public PrioritizedItem(T item_, float priority_)
    {
        item = item_;
        priority = priority_;
    }
}
