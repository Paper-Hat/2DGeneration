using UnityEngine;

namespace jkGenerator
{
    //Slap this onto your generator object
    public interface IGenerable
    {
        Generator.Constraints.Style Style { get; set; }
        Generator.Constraints.Types Types { get; set; }
        int NRooms { get; set; }
        (int, int) XRangeConstraint { get; set; }
        (int, int) YRangeConstraint { get; set; }
        Vector2 StartIndex { get; set; }
        void Awake();
    }
}