namespace Genetec.Dap
{
    using System;
    using System.Xml.Linq;

    public class RioKeepAlive
    {

        public TimeSpan Duration { get; set; }

        public RioKeepAlive(TimeSpan duration)
        {
            Duration = duration;
        }


        public override string ToString()
        {
            return new XElement("Request",
                new XElement("BusUpdate",
                    new XElement("StatusKeepalive",
                        new XElement("Duration", Duration)
                    )
                )
            ).ToString();
        }
    }
}
