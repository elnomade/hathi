#region Copyright (c) 2013 Hathi Project < http://hathi.sourceforge.net >
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
using System.IO;

namespace Hathi.eDonkey.Commands
{
	internal class CRequestSourceExchange
	{
		public byte[] FileHash;

		public CRequestSourceExchange(MemoryStream buffer, byte[] FileHash)
		{
			BinaryWriter writer = new BinaryWriter(buffer);
			DonkeyHeader header = new DonkeyHeader((byte)Protocol.ClientCommandExt.SourcesRequest, Protocol.ProtocolType.eMule);
			header.Packetlength = 17;
			header.Serialize(writer);
			writer.Write(FileHash);
			//          header.Packetlength=(uint)writer.BaseStream.Length-header.Packetlength+1;
			//          writer.Seek(0,SeekOrigin.Begin);
			//          header.Serialize(writer);
		}

		public CRequestSourceExchange(MemoryStream buffer)
		{
			BinaryReader reader = new BinaryReader(buffer);
			FileHash = reader.ReadBytes(16);
			reader.Close();
			buffer.Close();
			reader = null;
			buffer = null;
		}
	}

}
