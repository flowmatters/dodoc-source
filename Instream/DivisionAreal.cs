using FlowMatters.Source.DODOC.Core;
using RiverSystem;

namespace FlowMatters.Source.DODOC.Instream
{
    public class DivisionAreal : Areal
    {
        public DivisionAreal(Division d)
        {
            _division = d;
        }

        private Division _division;


        public double Area
        {
            get { return _division.Area; }
        }

        //public double PreviousArea
        //{
        //    get { return _division.PrevArea; }
        //}

        public override bool Equals(object obj)
        {
            return base.Equals(obj) || (obj is DivisionAreal ? ((DivisionAreal) obj)._division.Equals(_division):false);
        }

        public override int GetHashCode()
        {
            return _division.GetHashCode();
        }
    }
}
