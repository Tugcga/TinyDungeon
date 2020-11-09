using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    /*
     * Use this component to set collision edges for any non-static collidable object
     * gates, barrels (TODO) and so on
     * by using this component we can find indxes of collision edges and activate/deactivate it when neccessary
     */
    [GenerateAuthoringComponent]
    public struct CollisionEdgesSetComponent : IComponentData
    {
        //8 collision edges
        public int index01;
        public int index02;
        public int index03;
        public int index04;
        public int index05;
        public int index06;
        public int index07;
        public int index08;

        //default value = 0, this index never can be real index of the collision edge (bacause the node with index = 0 is a root node of the tree)
        public CollisionEdgesSetComponent(int i1, int i2, int i3, int i4, int i5, int i6, int i7, int i8)
        {
            index01 = i1;
            index02 = i2;
            index03 = i3;
            index04 = i4;
            index05 = i5;
            index06 = i6;
            index07 = i7;
            index08 = i8;
        }
    }
}