using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;

namespace OpenCV.Plaground.Models {
    internal sealed class OperationResult
    {
        private readonly ConcurrentQueue<LogRecord> log;
        
        public Image<Rgb, byte> Result { get; set; }

        public IEnumerable<LogRecord> Log
        {
            get { return log; }
        }

        public OperationResult()
        {
            log = new ConcurrentQueue<LogRecord>();
        }

        public OperationResult LogRecord<TColor, TDepth>(string text,  Image<TColor, TDepth> image)
            where TColor : struct, IColor where TDepth : new()
        {
            var now = DateTime.Now;

            log.Enqueue(new LogRecord()
            {
                Text = $"{now:HH:mm:ss.fff} {text}", 
                Image = image == null ? null : image.Convert<Rgb, byte>()
            });

            return this;
        }
        
        public OperationResult LogRecord<TColor, TDepth>(Image<TColor, TDepth> image)
            where TColor : struct, IColor where TDepth : new()
        {
            LogRecord($"Image({image.Size} Channels:{image.NumberOfChannels})", image);

            return this;
        }
        
        public OperationResult LogRecord(string text)
        {
            return LogRecord<Gray, byte>(text, null);
        }
    }
}