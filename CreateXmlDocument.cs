﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LSD_CLIENT_AQUA
{
    internal class CreateXmlDocument
    {
        

        public XDocument CreateXmlDoc(double x, double y, double angle)
        {
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("scanLSD",
                    new XAttribute("id", 1),
                    new XElement("valueX", x),
                    new XElement("valueY", y),
                    new XElement("valueAngle", angle)));

            doc.Save("scan.xml");
            return doc;
        }
        
    }
}