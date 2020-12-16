using System;

namespace CA.Platform.Exceptions
{
    public class NotAthenticatedException : Exception
    {
        public NotAthenticatedException() : base( "User not Authenticated")
        {
        }
    }
}