using System;
using System.Collections.Generic;
using System.Text;

namespace SportsExerciseBattle.REST_Server
{
    public interface IReqContext
    {
        string StatusCode { get; set; }
        string Payload { get; set; }
        string ContentType { get; set; }

        void RequestCoordinator();

    }
}
