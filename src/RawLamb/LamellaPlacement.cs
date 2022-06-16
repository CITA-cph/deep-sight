using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using System.Xml;

namespace RawLamb
{
    public class LamellaPlacement
    {
        public int LogIndex;
        public int BoardIndex;
        public bool Placed = false;
        public Plane Plane;
        public string Name;

        public LamellaPlacement(string name = "LamellaPlacement", int logIndex = -1, int boardIndex = -1)
        {
            Plane = Plane.Unset;
            Name = name;
            LogIndex = logIndex;
            BoardIndex = boardIndex;
        }

        public byte[] Serialize()
        {
            byte[] data = new byte[sizeof(int) + Name.Length + sizeof(int) * 2 + sizeof(bool) + sizeof(double) * 9];
            var index = 0;

            Array.Copy(BitConverter.GetBytes(Name.Length), 0, data, index, sizeof(int));
            index += sizeof(int);

            Array.Copy(Encoding.UTF8.GetBytes(Name), 0, data, index, Name.Length);
            index += Name.Length;

            Array.Copy(BitConverter.GetBytes(LogIndex), 0, data, index, sizeof(int));
            index += sizeof(int);

            Array.Copy(BitConverter.GetBytes(BoardIndex), 0, data, index, sizeof(int));
            index += sizeof(int);

            Array.Copy(BitConverter.GetBytes(Placed), 0, data, index, sizeof(bool));
            index += sizeof(bool);

            Array.Copy(BitConverter.GetBytes(Plane.Origin.X), 0, data, index, sizeof(double));
            index += sizeof(double);
            Array.Copy(BitConverter.GetBytes(Plane.Origin.Y), 0, data, index, sizeof(double));
            index += sizeof(double);
            Array.Copy(BitConverter.GetBytes(Plane.Origin.Z), 0, data, index, sizeof(double));
            index += sizeof(double);

            Array.Copy(BitConverter.GetBytes(Plane.XAxis.X), 0, data, index, sizeof(double));
            index += sizeof(double);
            Array.Copy(BitConverter.GetBytes(Plane.XAxis.Y), 0, data, index, sizeof(double));
            index += sizeof(double);
            Array.Copy(BitConverter.GetBytes(Plane.XAxis.Z), 0, data, index, sizeof(double));
            index += sizeof(double);

            Array.Copy(BitConverter.GetBytes(Plane.YAxis.X), 0, data, index, sizeof(double));
            index += sizeof(double);
            Array.Copy(BitConverter.GetBytes(Plane.YAxis.Y), 0, data, index, sizeof(double));
            index += sizeof(double);
            Array.Copy(BitConverter.GetBytes(Plane.YAxis.Z), 0, data, index, sizeof(double));
            index += sizeof(double);

            return data;
        }

        public static LamellaPlacement Deserialize(byte[] data)
        {
            var indexStep = 0;

            var nLength = BitConverter.ToInt32(data, indexStep);
            indexStep += sizeof(int);

            var name = Encoding.UTF8.GetString(data, indexStep, nLength);
            indexStep += nLength;

            var log_index = BitConverter.ToInt32(data, indexStep);
            indexStep += sizeof(int);

            var board_index = BitConverter.ToInt32(data, indexStep);
            indexStep += sizeof(int);

            var placed = BitConverter.ToBoolean(data, indexStep);
            indexStep += sizeof(bool);

            Point3d PtO = new Point3d(
              BitConverter.ToDouble(data, indexStep),
              BitConverter.ToDouble(data, indexStep + sizeof(double)),
              BitConverter.ToDouble(data, indexStep + sizeof(double) * 2)
              );
            indexStep += (sizeof(double) * 3);

            Vector3d Vx = new Vector3d(
              BitConverter.ToDouble(data, indexStep),
              BitConverter.ToDouble(data, indexStep + sizeof(double)),
              BitConverter.ToDouble(data, indexStep) + (sizeof(double) * 2)
              );
            indexStep += (sizeof(double) * 3);

            Vector3d Vy = new Vector3d(
              BitConverter.ToDouble(data, indexStep),
              BitConverter.ToDouble(data, indexStep + sizeof(double)),
              BitConverter.ToDouble(data, indexStep) + (sizeof(double) * 2)
              );

            var lp = new LamellaPlacement(name, log_index, board_index);
            Plane Pl = new Plane(PtO, Vx, Vy);
            lp.Placed = placed;
            lp.Plane = Pl;
            return lp;
        }

        public static byte[] SerializeMany(IEnumerable<LamellaPlacement> lps)
        {
            var Nbytes = 0;
            var datas = new List<byte[]>();
            foreach (var lp in lps)
            {
                Nbytes += sizeof(int);
                var lpdata = lp.Serialize();
                Nbytes += lpdata.Length;
                datas.Add(lpdata);
            }

            var data = new byte[Nbytes];
            int index = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(Nbytes), 0, data, index, sizeof(int));
            index += sizeof(int);

            foreach (var lpdata in datas)
            {
                Buffer.BlockCopy(lpdata, 0, data, index, lpdata.Length);
                index += lpdata.Length;
            }
            return data;
        }

        public static List<LamellaPlacement> DeserializeMany(byte[] data)
        {
            var lps = new List<LamellaPlacement>();
            int index = 0;
            var N = BitConverter.ToInt32(data, index);

            for (int i = 0; i < N; ++i)
            {
                var lpLength = BitConverter.ToInt32(data, index);
                index += sizeof(int);

                var lpdata = new byte[lpLength];
                Buffer.BlockCopy(data, index, lpdata, 0, lpLength);

                var lp = LamellaPlacement.Deserialize(lpdata);
                lps.Add(lp);
            }
            return lps;
        }

        public override string ToString()
        {
            return string.Format("LamellaPlacement({0} {1} {2} {3})", Name, LogIndex, BoardIndex, Placed);
        }

        public XmlElement ToXml(XmlDocument doc)
        {
            var main = doc.CreateElement("lamella_placement");

            var name = doc.CreateElement("name");
            name.InnerText = Name;
            main.AppendChild(name);

            var logindex = doc.CreateElement("log_index");
            logindex.InnerText = string.Format("{0}", LogIndex);
            main.AppendChild(logindex);

            var boardindex = doc.CreateElement("board_index");
            boardindex.InnerText = string.Format("{0}", BoardIndex);
            main.AppendChild(boardindex);

            var plane = doc.CreateElement("plane");
            var origin = doc.CreateElement("origin");
            origin.InnerText = string.Format("{0} {1} {2}", Plane.Origin.X, Plane.Origin.Y, Plane.Origin.Z);
            var xaxis = doc.CreateElement("xaxis");
            xaxis.InnerText = string.Format("{0} {1} {2}", Plane.XAxis.X, Plane.XAxis.Y, Plane.XAxis.Z);
            var yaxis = doc.CreateElement("yaxis");
            yaxis.InnerText = string.Format("{0} {1} {2}", Plane.YAxis.X, Plane.YAxis.Y, Plane.YAxis.Z);

            plane.AppendChild(origin);
            plane.AppendChild(xaxis);
            plane.AppendChild(yaxis);
            main.AppendChild(plane);

            return main;
        }

        public static LamellaPlacement FromXml(XmlElement element)
        {
            var name = element.SelectSingleNode("name").InnerText;
            var logIndex = int.Parse(element.SelectSingleNode("log_index").InnerText);
            var boardIndex = int.Parse(element.SelectSingleNode("board_index").InnerText);

            var plane = element.SelectSingleNode("plane");
            var originOut = plane.SelectSingleNode("origin");
            var xAxisOut = plane.SelectSingleNode("xaxis");
            var yAxisOut = plane.SelectSingleNode("yaxis");

            var originO = originOut.InnerText.Split();
            var xAxisO = xAxisOut.InnerText.Split();
            var yAxisO = yAxisOut.InnerText.Split();

            Point3d plOrigin = new Point3d(double.Parse(originO[0]), double.Parse(originO[1]), double.Parse(originO[2]));
            Vector3d VecX = new Vector3d(double.Parse(xAxisO[0]), double.Parse(xAxisO[1]), double.Parse(xAxisO[2]));
            Vector3d VecY = new Vector3d(double.Parse(yAxisO[0]), double.Parse(yAxisO[1]), double.Parse(yAxisO[2]));
            Plane pOut = new Plane(plOrigin, VecX, VecY);

            var lp = new LamellaPlacement(name, logIndex, boardIndex);
            lp.Plane = pOut;
            return lp;
        }

        public static List<LamellaPlacement> FromXml(List<XmlElement> elements)
        {
            var lps = new List<LamellaPlacement>();
            for (int i = 0; i < elements.Count; i++)
            {
                var lp = FromXml(elements[i]);
                lps.Add((lp));
            }
            return lps;
        }

        public static List<LamellaPlacement> FromXml(XmlNodeList elements)
        {
            var lps = new List<LamellaPlacement>();
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i] is XmlElement)
                {
                    var lp = LamellaPlacement.FromXml((XmlElement)elements[i]);
                    lps.Add(lp);
                }
            }
            return lps;
        }
    }
}
