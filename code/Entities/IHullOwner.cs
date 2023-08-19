

namespace EasyWeapons.Demo.Entities;

public interface IHullOwner
{
    BBox Hull { get; }
    void SetHull(BBox hull);
}
