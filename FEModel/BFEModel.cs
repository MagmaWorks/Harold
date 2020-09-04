//using BriefFiniteElementNet;
//using BriefFiniteElementNet.Elements;
//using BriefFiniteElementNet.Sections;
//using MWGeometry;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;
//using System.Text;
//using System.Threading.Tasks;

//namespace FEModel
//{
//    public class BFEModel
//    {
//        public Geometry modelGeo { get; set; }
//        public Loads modelLoads { get; set; }

//        public double beamDisc = 1;
//        public List<Node> Nodes = new List<Node>();
//        public List<BarElement> Beams = new List<BarElement>();
//        public Model model;
//        public int nLabel = 0;

//        public BFEModel(Geometry geom, Loads loads)
//        {
//            modelGeo = geom;
//            modelLoads = loads;
//        }


//        public FEResults Analyze()
//        {
//            // Initiating model
//            model = new Model();

//            // Create beam elements
//            CreateBeams();
//            // Create walls

//            // Create slabs

//            // Assemble model
//            AssembleModel();

//        }

//        public void AssembleModel()
//        {
//            foreach (var n in Nodes)
//                model.Nodes.Add(n);

//            foreach (var b in Beams)
//                model.Elements.Add(b);
//        }

//        public void CreateBeams()
//        {
//            for (int i = 0; i < modelGeo.Beams.Count; i++)
//            {
//                Beam b = modelGeo.Beams[i];

                
//            }
//        }

//        public void AddBeam(Beam b)
//        {
//            BarElement be = new BarElement(AddNode(b.Start), AddNode(b.End));

//            // specify beam kinematic
//            be.Behavior = BarElementBehaviour.BeamZTimoshenko;

//            // set releases
//            be.EndReleaseCondition = Constraints.MovementFixed;
//            be.StartReleaseCondition = Constraints.MovementFixed;

//            // set geometrical properties
//            var section = new UniformParametric1DSection();
//            section.

//            Beams.Add(be);
//        }
//        public Node AddNode(MWPoint3D pt)
//        {
//            Node n = new Node(pt.X, pt.Y, pt.Z) { Label = "n" + nLabel };
//            Nodes.Add(n);
//            nLabel++;

//            return n;
//        }

//    }
//}
