// 
// SectionView.cs
//  
// Author:
//       Tomasz Kubacki <Tomasz.Kubacki(at)gmail.com>
// 
// Copyright (c) 2010 Tomasz Kubacki 2010
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using MonoReports.Model.Controls;
using MonoReports.Core;
using MonoReports.Extensions.CairoExtensions;
using Cairo;
using MonoReports.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonoReports.ControlView
{
	public class SectionView : ControlViewBase
	{
		static Color blackColor = new Color (0, 0, 0);
		static Color lightGraykColor = new Color (0.67, 0.67, 0.66);
		static Color yellowColor = new Color (1, 1, 0);
		public Color SectionGripperColor;
		public bool IsCollapsed { get; set; }
		public static double SectionheaderHeight = 20;
		public static double SectionGripperHeight = 3;
		public Cairo.PointD SectionSpan { get; set; }
		public Cairo.PointD AbsoluteDrawingStartPoint { get; set; }
		public Rectangle GripperAbsoluteBound { get; set; }
		IControlViewFactory controlViewFactory;
		Report parentReport;
		Color sectionHeaderColor = new Color (0.9, 0.9, 0.97);
		public bool AllowCrossSectionControl {get;private set;}

		List<ControlViewBase> controls;
		public ReadOnlyCollection<ControlViewBase> Controls {
			get { return controls.AsReadOnly (); }
			private set {
				;
			}
		}


		public override Control ControlModel {
			get { return base.ControlModel; }
			set {
				base.ControlModel = value;
				section = value as Section;
			}
		}

		private Section section;

		public Section Section {
			get { return section; }
		}



		public SectionView (Report parentReport, IControlViewFactory controlViewFactory, Section section, Cairo.PointD sectionSpan) : base(section)
		{
			this.controlViewFactory = controlViewFactory;
			this.parentReport = parentReport;
			
			if(section is DetailSection)
				AllowCrossSectionControl = false;
			else
				AllowCrossSectionControl = true;
			
			SectionSpan = sectionSpan;		
			controls = new System.Collections.Generic.List<ControlViewBase> ();
			AddControls (this.section.Controls);
			SectionGripperColor = lightGraykColor;
			InvalidateBound ();
			
		}

		public ControlViewBase AddControl (Control controlToAdd){
			var controlView = controlViewFactory.CreateControlView (controlToAdd, this);
			controls.Add (controlView);
			return controlView;
		}
			
		public void AddControls (IList<Control> controlsToAdd)
		{
			for (int i = 0; i < controlsToAdd.Count; i++) {				
				AddControl(controlsToAdd[i]);				
			}
		}

		public void InvalidateBound ()
		{
			AbsoluteBound = new Rectangle (SectionSpan.X, SectionSpan.Y, section.Width, section.Height + SectionheaderHeight + SectionGripperHeight);
			GripperAbsoluteBound = new Rectangle (SectionSpan.X, SectionSpan.Y + section.Height + SectionheaderHeight, section.Width, SectionGripperHeight);
			AbsoluteDrawingStartPoint = new Cairo.PointD (AbsoluteBound.X, AbsoluteBound.Y + SectionheaderHeight);
		}

		#region implemented abstract members of MonoReport.ControlView.ControlViewBase

		public override Size Render (Cairo.Context c, bool render, bool isDesign)
		{
			Size size = new Size (parentReport.Width, section.Height);
			if (isDesign) {
				InvalidateBound ();
			} else {
				AbsoluteBound = new Rectangle (section.Location.X, section.Location.Y, section.Width, section.Height);
			}
			if (render) {
				c.Save ();
				c.FillRectangle (AbsoluteBound, section.BackgroundColor.ToCairoColor ());
				
				
				if (isDesign) {
					Rectangle r = new Rectangle (AbsoluteBound.X, AbsoluteBound.Y, parentReport.Width, SectionheaderHeight);
					c.FillRectangle (r, sectionHeaderColor);
					c.DrawText (new Cairo.PointD (r.X + 3, r.Y + 3), "Arial", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal, 12, blackColor, 600, Section.Name);
					c.FillRectangle (GripperAbsoluteBound, SectionGripperColor);
					c.Translate (AbsoluteDrawingStartPoint.X, AbsoluteDrawingStartPoint.Y);
					for (int j = 0; j < Controls.Count; j++) {
						var ctrl = Controls[j];
						ctrl.Render (c, render, true);
					}
				}
				
				
				c.Restore ();
			}
			
			
			
			return size;
		}


		public override bool ContainsPoint (double x, double y)
		{
			return AbsoluteBound.ContainsPoint (x, y);
		}

		#endregion
		
		public PointD PointInSectionByAbsolutePoint(PointD absolutePoint){
			return PointInSectionByAbsolutePoint(absolutePoint.X,absolutePoint.Y);
		}
		
		public PointD PointInSectionByAbsolutePoint(double x, double y){
			return new PointD(x - AbsoluteDrawingStartPoint.X, y - AbsoluteDrawingStartPoint.Y);
		}
		
		public PointD AbsolutePointByLocalPoint(double x, double y){
			return new PointD(x + AbsoluteDrawingStartPoint.X, y + AbsoluteDrawingStartPoint.Y);
		}

		private bool sectionGripperHighlighted;
		public bool SectionGripperHighlighted {


			get { return sectionGripperHighlighted; }

			set {
				if (value != sectionGripperHighlighted) {
					sectionGripperHighlighted = value;
					if (sectionGripperHighlighted) {
						SectionGripperColor = yellowColor;
					} else {
						SectionGripperColor = lightGraykColor;
					}
				}
			}
		}
		
	}
}
