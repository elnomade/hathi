#region Copyright (c)2009 Hathi Project < http://hathi.sourceforge.net >
/*
* This file is part of Hathi Project
* Hathi Developers Team:
* andrewdev, beckman16, biskvit, elnomade_devel, ershyams, grefly, jpierce420,
* knocte, kshah05, manudenfer, palutz, ramone_hamilton, soudamini, writetogupta
*
* Hathi is a fork of Lphant Version 1.0 GPL
* Lphant Team
* Juanjo, 70n1, toertchn, FeuerFrei, mimontyf, finrold, jicxicmic, bladmorv,
* andrerib, arcange|, montagu, wins, RangO, FAV, roytam1, Jesse
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either
* version 2 of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using Hathi.Types;

namespace Hathi.eDonkey
{
	internal class CCategoriesList
	{
		public ArrayList List;
		private string m_xmlRootNode;
		private string m_version;
		private string m_currentversion;
		private string m_configDirectory;
		private string m_fileName;

		public string this[uint ID]
		{
			get
			{
				CCategory category;
				for (int i = 0; i < List.Count; i++)
				{
					category = (CCategory)List[i];
					if (category.ID == ID) return category.Name;
				}
				return "";
			}
		}

		public CCategoriesList()
		{
			m_xmlRootNode = "HathiCategoriest";
			m_version = "0.1";
			m_configDirectory = Application.StartupPath;
			m_fileName = "categories.xml";
			List = new ArrayList();
			m_Load();
		}

		private void m_Load()
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(this.m_configDirectory + Path.DirectorySeparatorChar + this.m_fileName);
				m_currentversion = doc.DocumentElement.Attributes["version"].InnerText;
				m_SetValueFromXmlElement(doc.DocumentElement["Categories"]);
			}
			catch
			{
			}
		}

		private void m_SetValueFromXmlElement(XmlElement element)
		{
			try
			{
				XmlNodeList nodes = element.ChildNodes;
				foreach (XmlElement el in nodes)
				{
					if (el.Name == "Category")
					{
						CCategory category = new CCategory();
						category.Name = el.Attributes["Name"].InnerText;
						category.AutoString = el.Attributes["AutoString"].InnerText;
						category.Color = uint.Parse(el.Attributes["Color"].InnerText);
						category.ID = uint.Parse(el.Attributes["ID"].InnerText);
						List.Add(category);
					}
				}
			}
			catch
			{
				CLog.Log(Constants.Log.Info, "Error loading categories list");
			}
		}

		public bool Save()
		{
			lock (this)
			{
				try
				{
					XmlDocument doc = new XmlDocument();
					doc.LoadXml("<?xml version=\"1.0\"?>\n<" + this.m_xmlRootNode + " version = \"" + this.m_version + "\" />");
					doc.DocumentElement.AppendChild(m_ToXmlElement(doc));
					doc.Save(this.m_configDirectory + Path.DirectorySeparatorChar + this.m_fileName);
				}
				catch
				{
					return false;
				}
				return true;
			}
		}

		private XmlElement m_ToXmlElement(XmlDocument doc)
		{
			XmlElement categoriesNode = doc.CreateElement("Categories");
			XmlAttribute key;
			foreach (CCategory category in List)
			{
				XmlElement el = doc.CreateElement("Category");
				key = doc.CreateAttribute("Name");
				key.InnerText = category.Name;
				el.Attributes.Append(key);
				key = doc.CreateAttribute("AutoString");
				key.InnerText = category.AutoString;
				el.Attributes.Append(key);
				key = doc.CreateAttribute("Color");
				key.InnerText = category.Color.ToString();
				el.Attributes.Append(key);
				key = doc.CreateAttribute("ID");
				key.InnerText = category.ID.ToString();
				el.Attributes.Append(key);
				categoriesNode.AppendChild(el);
			}
			return categoriesNode;
		}

		public bool AddorUpdateCategory(uint ID, string name, string auto, uint color)
		{
			uint newID = ID;
			CCategory newCategory = null;
			//if ID>0 check if already exists
			if (newID > 0)
			{
				foreach (CCategory category in List)
				{
					if (category.ID == newID) newCategory = category;
				}
			}
			else
			{
				uint maxID = 1;
				foreach (CCategory category in List)
				{
					if (category.ID >= maxID) maxID = category.ID + 1;
				}
				newID = maxID;
			}
			if (newCategory == null)
			{
				newCategory = new CCategory();
				List.Add(newCategory);
			}
			newCategory.ID = newID;
			newCategory.Name = name;
			newCategory.Color = color;
			newCategory.AutoString = auto;
			return true;
		}

		public bool RemoveCategory(uint ID)
		{
			foreach (CCategory category in List)
			{
				if (category.ID == ID)
				{
					List.Remove(category);
					return true;
				}
			}
			return false;
		}

		public CCategory GetCategoryByName(string name)
		{
			foreach (CCategory category in List)
			{
				if (category.Name == name)
				{
					return category;
				}
			}
			return null;
		}

		public uint GetAutoCategory(string fileName)
		{
			foreach (CCategory category in List)
			{
				if (category.AutoString.Length > 0)
				{
					string[] matchWords = category.AutoString.Split(' ');
					foreach (string word in matchWords)
					{
						if (fileName.ToLower().IndexOf(word.ToLower()) >= 0)
						{
							return category.ID;
						}
					}
				}
			}
			return 0;
		}
	}
}
