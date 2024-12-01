using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace NCP
{
    public class NCPComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public NCPComponent()
          : base("NextClosestPoint", 
                "NCP",
                "recursive algorithm that finds the next n closest points",
                "Paws", 
                "points")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("candidates", "c", "possible positions of new points", GH_ParamAccess.list);
            pManager.AddPointParameter("obstacles", "o", "obstacle points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("steps", "n", "number of steps", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("centres", "c", "new centres", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            /// Point cloud to search from
            List<Point3d> c = new List<Point3d>();

            /// focus (obstacle) points to find the closest points to 
            List<Point3d> o = new List<Point3d>();

            // Number of recursion calls (number of iterations)
            int s = new int();

            /// Fetch inputs
            DA.GetDataList(0, c);
            DA.GetDataList(1, o);
            DA.GetData(2, ref s);

            /// main recursive function to find next closest points
            List<Point3d> supps(int steps, List<Point3d> candidates, List<Point3d> obstacles)
            {
                ///
                /// Each recursive call finds the point in the point cloud which has the least combined distance to all obstacle points and adds that point to the obstacle point to use for the next recursion call   
                ///
                if (obstacles.Count == steps)
                {
                    return obstacles;
                }

                /// store new obstacle points
                List<Point3d> newObstacles = new List<Point3d>();
                foreach (Point3d point in obstacles)
                {
                    newObstacles.Add(point);
                }

                int newIdx = 0;

                double minGap = 0.0;

                for (int i = 0; i < candidates.Count; i++)
                {
                    double gap = 1.0;
                    for (int j = 0; j < obstacles.Count; j++)
                    {
                        gap *= obstacles[j].DistanceTo(candidates[i]);
                    }
                    if (minGap == 0.0 || gap < minGap && !obstacles.Contains(candidates[i]))
                    {
                        minGap = gap;
                        newIdx = i;
                    }
                }

                newObstacles.Add(candidates[newIdx]);

                return supps(steps, candidates, newObstacles);
            }

            List<Point3d> newSupps = supps(s + o.Count, c, o);

            DA.SetDataList(0, newSupps);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("852d4108-01f6-4aa4-b28f-3a68d932c69a");
    }
}
