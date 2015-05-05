using System;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace FootballAnalyzerWindows.Common
{
    public sealed class DialControl : Control
    {
        private Ellipse m_playPause;
        private Ellipse m_playPauseCircle;
        private Image m_playPauseImage;
        private Ellipse m_dial;
        private Ellipse m_dialCircle;       

        public DialControl()
        {
            this.DefaultStyleKey = typeof(DialControl);            
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_playPause = this.GetTemplateChild("PlayPause") as Ellipse;
            m_playPause.Height = this.Height / 3.75;
            m_playPause.Width = this.Height / 3.75;
            m_playPause.PointerPressed += (o, e) =>
                {
                    this.PlayPausePressed(this, null);
                };

            m_playPauseCircle = this.GetTemplateChild("PlayPauseCircle") as Ellipse;
            m_playPauseCircle.Height = this.Height / 3.75;
            m_playPauseCircle.Width = this.Height / 3.75;

            m_playPauseImage = this.GetTemplateChild("PlayPauseImage") as Image;
            m_playPauseImage.Height = this.Height / 5;
            m_playPauseImage.Width = this.Width / 5;

            m_dial = this.GetTemplateChild("Dial") as Ellipse;
            m_dial.ManipulationStarted += (o, e) => 
                { 
                    this.DialManipulationStarted(this, null); 
                };
            m_dial.ManipulationCompleted += (o, e) =>
                {
                    m_havePreviousAngle = false;
                };
            m_dial.ManipulationDelta += DialManipulationDelta;

            m_dialCircle = this.GetTemplateChild("DialCircle") as Ellipse;
            m_dialCircle.StrokeThickness = this.Height / 25;
            m_dialCircle.Height = this.Height / 1.25;
            m_dialCircle.Width = this.Width / 1.25;

            this.m_dialCenter = new Point(this.Width / 2, this.Height / 2);
        }

        private double m_previousAngle = 0;
        private bool m_havePreviousAngle = false;
        private double m_accumulatedTimeDelta;
        private Point m_dialCenter;
        private void DialManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            
            if (e.Position.Distance(m_dialCenter) < m_playPause.ActualHeight / 2)
            {
                // If we run get a point to close to the center, ignore it and the previous remembered point
                m_havePreviousAngle = false;
            }
            else
            {
                var angle = Math.Atan2(m_dialCenter.Y - e.Position.Y, e.Position.X - m_dialCenter.X);     
                if (angle < 0)
                {
                    angle += (2 * Math.PI);
                }

                if (m_havePreviousAngle)
                {
                    var delta = m_previousAngle - angle;
                    
                    // Account for going around the circle completely
                    if (delta > Math.PI)
                    {
                        delta -= (Math.PI * 2);
                    }
                    else if (delta < Math.PI * -1)
                    {
                        delta += (Math.PI * 2);
                    }

                    //
                    // control video
                    // normal playback = 5s / 360 degrees
                    //
                    m_accumulatedTimeDelta += (5000 * delta / (2 * Math.PI));
                    if (Math.Abs(m_accumulatedTimeDelta) > 25)
                    {
                        this.DialTimeDelta(this, m_accumulatedTimeDelta);
                        m_accumulatedTimeDelta = 0;
                    }
                }

                m_previousAngle = angle;
                m_havePreviousAngle = true;
            }
            e.Handled = true;
        }

        public event EventHandler PlayPausePressed;

        public event EventHandler<double> DialTimeDelta;

        public event EventHandler DialManipulationStarted;
    }
}
