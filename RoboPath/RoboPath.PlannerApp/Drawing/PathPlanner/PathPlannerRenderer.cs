//// *******************************************************
//// Project: RoboPath.PlannerApp
//// File Name: PathPlannerRenderer.cs
//// By: Frank Perks
//// *******************************************************

//using System.Windows.Media;

//using NLog;

//using RoboPath.Core;
//using RoboPath.Core.Graph;
//using RoboPath.Core.PathPlanning;
//using RoboPath.PlannerApp.Properties;

//namespace RoboPath.PlannerApp.Drawing.PathPlanner
//{
//    public class PathPlannerRenderer<T>
//        where T : class, IPathPlanner
//    {
//        #region Fields

//        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


//        #endregion Fields

//        #region Properties

//        public IRenderer ParentRenderer { get; private set; }
//        public T PathPlanner { get; private set; }

//        #endregion Properties

//        #region Public Methods

//        public PathPlannerRenderer(T pathPlanner, IRenderer renderer)
//        {
//            PathPlanner = pathPlanner;
//            ParentRenderer = renderer;
//        }

//        public virtual void Draw(DrawingVisual visual)
//        {
//            using(var context = visual.RenderOpen())
//            {

//            }
//        }

//        #endregion Public Methods

//        #region Internal Methods

//        protected virtual void DrawEdges()
//        {

//        }

//        protected virtual void DrawVertices(DrawingVisual visual)
//        {
//            if(PathPlanner == null)
//            {
//                return;
//            }

//            using(var context = visual.RenderOpen())
//            {
//                var roadmap = PathPlanner.Graph;
//                context.DrawCoordinates(roadmap.Vertices, Renderer.GraphVertexBrush, Renderer.GraphVertexPen, GeometricShape.Circle, Settings.Default.DrawingVertexRadius);

//                if(PathPlanner.State == PathPlannerAlgorithmState.ShortestPathFound)
//                {
//                    var vertices = PathPlanner.ShortestPath.GetEdgeVertices();
//                    context.DrawCoordinates(vertices, Renderer.GraphShortestPathVertexBrush, Renderer.GraphVertexPen, GeometricShape.Circle, Settings.Default.DrawingShortestPathVertexRadius);
//                }
//            }
//        }

//        #endregion Internal Methods
//    }
//}