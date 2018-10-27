using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using URandom = UnityEngine.Random;
using CRandom = System.Random;

public static class Randomization {

    #region Randomization Functions
    //Accessible generation seed
    public static int Seed;

    /// <summary>
    /// Using given list, returns random element from list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T GetRandom<T>(List<T> list)
    {
        return list[URandom.Range(0, list.Count)];
    }

    /// <summary>
    /// Returns a random room from a list of rooms given rooms are weighted
    /// </summary>
    /// <param name="weightedRooms"></param>
    /// <returns></returns>
    public static Room GetRandomWeightedRoom(List<Room> weightedRooms)
    {
        var weightedRand = URandom.Range(0, TotalWeight(weightedRooms));
        foreach (var weightedRoom in weightedRooms)
        {
            if (weightedRand < weightedRoom.Weight)
                return weightedRoom;
            weightedRand -= weightedRoom.Weight;
        }
        return null;

    }
    /// <summary>
    /// "True" randomization based on bitwise and of current date/time using C# random int function
    /// </summary>
    /// <returns></returns>
    private static int GenerateCRandom()
    {
        int rand = (int)(DateTime.Now.Ticks & 0x0000FFFF);
        CRandom seed = new CRandom(rand);
        return seed.Next();
    }

    /// <summary>
    /// Generates unity seed using C# random method, otherwise using user-init seed.
    /// </summary>
    /// <param name="newSeed"></param>
    public static void GenerateUnitySeed(int newSeed)
    {
        Seed = (newSeed == 0) ? GenerateCRandom() : newSeed;
        Debug.Log("Seed: " + Seed);
        URandom.InitState(Seed);
    }
    #endregion

    #region Helper Functions
    /// <summary>
    /// Sums total weight given list of rooms
    /// </summary>
    /// <param name="weightedRooms"></param>
    /// <returns></returns>
    private static int TotalWeight(List<Room> weightedRooms)
    {
        int totalWeight = 0;
        weightedRooms = weightedRooms.Where(x => x.Weight > 0).ToList();
        foreach (var room in weightedRooms)
            totalWeight += room.Weight;
        return totalWeight;
    }
    #endregion
}
