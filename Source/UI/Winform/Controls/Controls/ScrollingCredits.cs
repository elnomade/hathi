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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Hathi.UI.Winform.Controls
{
/// <summary>
/// ScrollingCredits is a control to display Hathi members.
/// </summary>
public class ScrollingCredits : System.Windows.Forms.Control
{
    /// <summary>
    /// Members.
    /// </summary>
    private System.ComponentModel.Container components = null;
    private Timer m_Timer;
    private ArrayList m_HathiTeam;
    private float Step=0F;
    private ScrollingCredits.CStyle Header1Style;
    private ScrollingCredits.CStyle Header2Style;
    private ScrollingCredits.CStyle Header3Style;
    private ScrollingCredits.CStyle Header4Style;

    public enum Role
    {
        None,
        Translator,
        Developer,
        Boss,
    }

    public enum Style
    {
        Header1,
        Header2,
        Header3,
        Header4,
    }

    public ScrollingCredits()
    {
        // Required for Windows Form Designer support
        InitializeComponent();
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        this.SetStyle(ControlStyles.UserPaint, true);
        this.SetStyle(ControlStyles.DoubleBuffer, true);
        this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        this.BackColor=Color.Transparent;
        m_HathiTeam = new ArrayList();
        m_CreateStyle();
        m_CreateHathiTeam();
        m_Timer = new Timer();
        m_Timer.Interval = 30;
        m_Timer.Enabled = true;
        m_Timer.Tick += new EventHandler(Animate);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
        if ( disposing )
        {
            if ( components != null )
                components.Dispose();
        }
        base.Dispose( disposing );
    }

    #region Component designer generated code 
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
    }
    #endregion


    protected override void OnPaint(PaintEventArgs pe)
    {
        float textHeight=0;
        SizeF textSize=SizeF.Empty;
        this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.SupportsTransparentBackColor,true);
        this.BackColor=Color.Transparent;
        // TODO�: personnalized painting
        RectangleF textRect = new RectangleF();
        foreach (CTeamInfo ti in m_HathiTeam)
        {
            switch (ti.Style)
            {
            case Style.Header1:
                textSize=pe.Graphics.MeasureString(ti.Name, Header1Style.FontStyle);
                break;
            case Style.Header2:
                textSize=pe.Graphics.MeasureString(ti.Name, Header2Style.FontStyle);
                break;
            case Style.Header3:
                textSize=pe.Graphics.MeasureString(ti.Name, Header3Style.FontStyle);
                break;
            case Style.Header4:
                textSize=pe.Graphics.MeasureString(ti.Name, Header4Style.FontStyle);
                break;
            }
            //SizeF textSize = pe.Graphics.MeasureString(ti.Name, Header1Style.FontStyle);
            textRect.Width = ClientRectangle.Right;
            textRect.Height = textSize.Height;
            textRect.X = ClientRectangle.X;
            textRect.Y = Step + ClientRectangle.Height+textHeight;// - textRect.Height) / 2;
            switch (ti.Style)
            {
            case Style.Header1:
                pe.Graphics.DrawString(ti.Name,Header1Style.FontStyle,Header1Style.BrushStyle,textRect,Header1Style.StringFormatStyle);
                break;
            case Style.Header2:
                pe.Graphics.DrawString(ti.Name,Header2Style.FontStyle,Header2Style.BrushStyle,textRect,Header2Style.StringFormatStyle);
                break;
            case Style.Header3:
                pe.Graphics.DrawString(ti.Name,Header3Style.FontStyle,Header3Style.BrushStyle,textRect,Header3Style.StringFormatStyle);
                break;
            case Style.Header4:
                pe.Graphics.DrawString(ti.Name,Header4Style.FontStyle,Header4Style.BrushStyle,textRect,Header4Style.StringFormatStyle);
                break;
            }
            textHeight += textSize.Height;
        }
        Step=Step-1F;
        if (Step<-(Height+textHeight))
            Step=0;
        base.OnPaint(pe);
    }

    private void Animate( object sender, EventArgs e )
    {
        this.Invalidate();
    }

    private void m_CreateHathiTeam()
    {
        m_HathiTeam.Add(new CTeamInfo("Hathi",Role.None,Style.Header1));
        m_HathiTeam.Add(new CTeamInfo("Copyright (C)2009 Hathi Team", Role.None, Style.Header2));
        m_HathiTeam.Add(new CTeamInfo(" ",Role.None,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo(" ",Role.None,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("Hathi Team", Role.None, Style.Header3));
        m_HathiTeam.Add(new CTeamInfo("alexman", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("andrewdev", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("armando-b", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("beckman16", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("biskvit", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("elnomade", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("grefly", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("jol1naras", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("jpierce420", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("loriss", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("manudenfer", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("palutz", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("ramone_hamilton", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("sdaniele", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("soudamini", Role.Developer, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("webetes", Role.Developer, Style.Header4));

        m_HathiTeam.Add(new CTeamInfo(" ", Role.None, Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("lphant Team", Role.None, Style.Header3));

        m_HathiTeam.Add(new CTeamInfo("Developers",Role.None,Style.Header3));
        m_HathiTeam.Add(new CTeamInfo("Juanjo",Role.Developer,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("70n1",Role.Developer,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("toertchn",Role.Developer,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("FeuerFrei",Role.Developer,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("mimontyf",Role.Developer,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("finrold",Role.Developer,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("jicxicmic",Role.Developer,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo(" ",Role.None,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("Translators",Role.None,Style.Header3));
        m_HathiTeam.Add(new CTeamInfo("bladmorv : German",Role.Translator,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("andrerib : Brazilian",Role.Translator,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("arcange| : Galizian",Role.Translator,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("montagu : Catalan",Role.Translator,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("wins : Polish",Role.Translator,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("RangO : Italian",Role.Translator,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("FAV : Russian",Role.Translator,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("roytam1 : Traditional chinese ",Role.Translator,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("Jesse : Finnish",Role.Translator,Style.Header4));
        m_HathiTeam.Add(new CTeamInfo("Decayer-9 : Hungarian",Role.Translator,Style.Header4));
    }

    private void m_CreateStyle()
    {
        StringFormat centerFormat = new StringFormat();
        centerFormat.Alignment = StringAlignment.Center;
        Header1Style=new CStyle(new Font("Microsoft Sans Serif",20F,FontStyle.Bold),
                                new SolidBrush(Color.Red),
                                centerFormat);
        Header2Style=new CStyle(new Font("Microsoft Sans Serif",12F,FontStyle.Bold),
                                new SolidBrush(Color.Red),
                                centerFormat);
        Header3Style=new CStyle(new Font("Microsoft Sans Serif",12F,FontStyle.Bold),
                                new SolidBrush(Color.Red),
                                centerFormat);
        Header4Style=new CStyle(new Font("Microsoft Sans Serif",8.25F,FontStyle.Bold),
                                new SolidBrush(Color.White),
                                centerFormat);
    }

    private class CStyle
    {
        private Font m_Font;
        private Brush m_Brush;
        private StringFormat m_StringFormat;

        public Font FontStyle
        {
            get {
                return m_Font;
            }
        }

        public Brush BrushStyle
        {
            get {
                return m_Brush;
            }
        }

        public StringFormat StringFormatStyle
        {
            get {
                return m_StringFormat;
            }
        }

        public CStyle(Font in_Font,Brush in_Brush,StringFormat in_StringFormat)
        {
            m_Font=in_Font;
            m_Brush=in_Brush;
            m_StringFormat=in_StringFormat;
        }
    }

    private class CTeamInfo
    {
        private string m_Name;
        private Role m_Role;
        private Style m_Style;

        public string Name
        {
            get {
                return m_Name;
            }
        }

        public Role Role
        {
            get {
                return m_Role;
            }
        }

        public Style Style
        {
            get {
                return m_Style;
            }
        }

        public CTeamInfo(string in_Name, Role in_Role, Style in_Style)
        {
            m_Name=in_Name;
            m_Role=in_Role;
            m_Style=in_Style;
        }

    }
}
}
