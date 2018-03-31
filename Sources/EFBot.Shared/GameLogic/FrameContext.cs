using System;
using System.Collections.Generic;
using EFBot.Shared.GameLogic.ImageReaders;
using OpenTK.Platform.Windows;

namespace EFBot.Shared.GameLogic
{
    internal sealed class FrameContext
    {
        private readonly IDictionary<Type, ImageReaderBase> valuesByFeatureType = new Dictionary<Type, ImageReaderBase>();
        
        public TValue AddOrUpdateReader<TValue>(TValue value) where TValue : ImageReaderBase
        {
            valuesByFeatureType[value.GetType()] = value;
            return value;
        }
        
        public TReader GetOrDefault<TReader>() where TReader : ImageReaderBase
        {
            ImageReaderBase rawResult;
            if (!valuesByFeatureType.TryGetValue(typeof(TReader), out rawResult))
            {
                return default(TReader);
            }
            return (TReader)rawResult;
        }
    }
}