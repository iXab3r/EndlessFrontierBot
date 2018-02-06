﻿using System;
using System.Drawing;
using System.Linq;
using DynamicData;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using Emgu.CV;
using Emgu.CV.Structure;

namespace EFBot.Shared.GameLogic.ImageReaders {
    internal sealed class UnitsListReader : ImageReaderBase<UnitShopUnit[]>
    {
        public UnitsListReader(IRecognitionEngine recognitionEngine, IGameImageSource imgSource) : base(recognitionEngine, imgSource) { }

        public override void Refresh(Image<Bgr, byte> inputImage)
        {
            Clear();
            
            if (inputImage.Size.IsEmpty)
            {
                Text = "Window not found";
                return;
            }

            var units = ImageSource.UnitNameAreas
                .Zip(ImageSource.UnitPriceAreas, (nameArea, priceArea) => new {nameArea, priceArea})
                .Select(
                    (x, idx) =>
                    {
                        var nameImg = inputImage.GetSubRect(x.nameArea);
                        
                        // remove Unit Star rating
                        var starMask = nameImg.Convert<Hsv, byte>()
                            .Split()[0]
                            .InRange(new Gray(10), new Gray(60))
                            .Convert<Bgr, byte>(); 

                        nameImg = nameImg.Sub(starMask);
                        
                        var name = RecognitionEngine.Recognize(nameImg);

                        var priceImg = inputImage.GetSubRect(x.priceArea);
                        var price = RecognitionEngine.Recognize(priceImg);
                        
                        inputImage.Draw(x.nameArea, new Bgr(Color.Yellow), 1);
                        inputImage.Draw(x.priceArea, new Bgr(Color.Yellow), 1);
                        
                        RecognitionResults.Add(name);
                        RecognitionResults.Add(price);
                    
                        return new UnitShopUnit()
                        {
                            Idx = idx,
                            Price = price.Text,
                            Name = name.Text,
                        };
                    })
                .Where(IsValid)
                .ToArray();

            Entity = units;
        }
        
        private bool IsValid(UnitShopUnit unit)
        {
            return IsValidPriceText(unit.Price);
        }

        private bool IsValidPriceText(string text)
        {
            int price;
            if (!Int32.TryParse(text, out price))
            {
                return false;
            }

            return price > 0 && price <= 15000;
        }
    }
}