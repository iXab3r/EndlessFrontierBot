using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;

namespace OpenCV.Plaground.Models {
    internal sealed class OperationResult
    {
        private readonly ConcurrentQueue<string> log;
        
        public Image<Rgb, byte> Result { get; set; }

        public IEnumerable<string> Log
        {
            get { return log; }
        }

        public OperationResult()
        {
            log = new ConcurrentQueue<string>();
        }

        public OperationResult LogRecord(string text)
        {
            var now = DateTime.Now;
            log.Enqueue($"{now:HH:mm:ss.fff} {text}");

            return this;
        }
    }
}