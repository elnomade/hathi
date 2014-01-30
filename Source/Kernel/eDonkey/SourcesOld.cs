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
using System.Collections;

namespace Hathi.eDonkey
{
internal class CSourceOld
{
    public byte[] FileHash;
    public byte[] UserHash;
    public byte[] FileChunks;
    public DateTime DeletedTime;

    public CSourceOld(CClient client)
    {
        FileHash=client.DownFileHash;
        UserHash=client.UserHash;
        FileChunks=client.DownFileChunks;
        DeletedTime=DateTime.Now;
    }
}
/// <summary>
/// List of old sources
/// </summary>
internal class CSourcesOld
{
    private ArrayList m_ArrayList;
    private TimeSpan m_diffTime;
    private TimeSpan m_diffCleaningTime;
    private DateTime m_LastCleaned;

    public CSourcesOld()
    {
        m_ArrayList=ArrayList.Synchronized(new ArrayList());
        m_diffTime=new TimeSpan(0,50,0);
        m_diffCleaningTime=new TimeSpan(0,10,0);
        m_LastCleaned=DateTime.MinValue;
    }

    public CSourceOld GetSourceOld(byte[] UserHash)
    {
        lock (m_ArrayList.SyncRoot)
        {
            foreach (CSourceOld sourceOld in m_ArrayList)
            {
                if (CKernel.SameHash(ref UserHash, ref sourceOld.UserHash)) return sourceOld;
            }
        }
        return null;
    }

    public bool AddClient(CClient client)
    {
        if (client.DownFileChunks==null) return false;
        CSourceOld source=GetSourceOld(client.UserHash);
        if (source==null)
        {
            CSourceOld sourceOld=new CSourceOld(client);
            m_ArrayList.Add(sourceOld);
            return true;
        }
        else
            source.DeletedTime=DateTime.Now;
        return false;
    }

    public void CleanOldSources()
    {
        if (DateTime.Now-m_LastCleaned<m_diffCleaningTime)
            return;
        m_LastCleaned=DateTime.Now;
        ArrayList toDelete=null;
        lock (m_ArrayList.SyncRoot)
        {
            foreach (CSourceOld sourceOld in m_ArrayList)
            {
                if (DateTime.Now-sourceOld.DeletedTime>m_diffTime)
                {
                    if (toDelete==null) toDelete=new ArrayList();
                    toDelete.Add(sourceOld);
                }
            }
        }
        if (toDelete!=null)
        {
            foreach (CSourceOld sourceOld in toDelete)
            {
                m_ArrayList.Remove(sourceOld);
                //CLog.Log(Types.Constants.Log.Verbose,"Source cleanes"+m_ArrayList.Count.ToString());
            }
        }
    }
}
}
