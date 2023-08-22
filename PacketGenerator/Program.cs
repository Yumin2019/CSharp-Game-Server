using System;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static string genPackets = "";

        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            using(XmlReader r = XmlReader.Create("PDL.xml", settings))
            {
                r.MoveToContent();

                while(r.Read())
                {
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);
                    Console.WriteLine("name = " + r.Name + " " + r["name"]);
                }
            }

            File.WriteAllText("GenPackets.cs", genPackets);
        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement)
                return;

            if (r.Name.ToLower() != "packet")
            {
                Console.WriteLine("invalid packet node");
                return;
            }

            string packetName = r["name"];
            if(string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("packet without name");
                return;
            }

            var tuple3 = ParseMember(r);
            genPackets += string.Format(PacketFormat.packetFormat, packetName, tuple3.Item1, tuple3.Item2, tuple3.Item3);
        }

        // {1} 멤버 변수들
        // {2} 멤버변수 Read
        // {3} 멤버변수 Write
        private static Tuple<string, string, string> ParseMember(XmlReader r)
        {
            string packetName = r["name"];
            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            int depth = r.Depth + 1;
            while(r.Read())
            {
                // 상위 packet 클래스 내부에 있는 애들만 처리
                if (r.Depth != depth)
                    break;

                string memberName = r["name"];
                if(string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("member without name");
                    return null;
                }

                if(string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine;
                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine;
                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine;

                string memberType = r.Name.ToLower();
                switch(memberType)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string> tuple = ParseList(r);
                        memberCode += tuple.Item1;
                        readCode += tuple.Item2;
                        writeCode += tuple.Item3;
                        break;
                }
            }

            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseList(XmlReader r)
        {
            string listName = r["name"];
            if(string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }

            Tuple<string, string, string> tuple3 = ParseMember(r);
            string memberCode = string.Format(PacketFormat.memberListFormat, 
                FirstCharToUpper(listName), 
                FirstCharToLower(listName), 
                tuple3.Item1, 
                tuple3.Item2, 
                tuple3.Item3);

            string readCode = string.Format(PacketFormat.readListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            string writeCode = string.Format(PacketFormat.writeListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                case "string":
                    break;
                case "list":
                    break;
            }

            return "";
        }
       
        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }

}