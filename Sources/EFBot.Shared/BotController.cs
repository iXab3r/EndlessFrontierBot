using System;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using Emgu.CV;
using Emgu.CV.Structure;
using ReactiveUI;

namespace EFBot.Shared {
    public sealed class BotController : ReactiveObject
    {
        private readonly GameImageSource gameSource;
        private Bitmap botImage;
        private UnitShopUnit[] availableUnits;
        private string recognizedText;
        private TimeSpan? timeLeftTillRefresh;

        private readonly RecognitionEngine recognitionEngine = new RecognitionEngine();

        public BotController(GameImageSource gameSource)
        {
            this.gameSource = gameSource;
            gameSource.WhenAnyValue(x => x.Source)
                .Subscribe(Recalculate);
        }

        public Bitmap BotImage
        {
            get { return botImage; }
            set { this.RaiseAndSetIfChanged(ref botImage, value); }
        }

        public UnitShopUnit[] AvailableUnits
        {
            get { return availableUnits; }
            set { this.RaiseAndSetIfChanged(ref availableUnits, value); }
        }

        public TimeSpan? TimeLeftTillRefresh
        {
            get { return timeLeftTillRefresh; }
            set { this.RaiseAndSetIfChanged(ref timeLeftTillRefresh, value); }
        }

        public string RecognizedText
        {
            get { return recognizedText; }
            set { this.RaiseAndSetIfChanged(ref recognizedText, value); }
        }
        
        private void Recalculate(Bitmap activeImage)
        {
            if (activeImage == null)
            {
                BotImage = null;
                return;
            }
            var allText = new StringBuilder();

            var roiImage = new Image<Bgr, byte>(activeImage);
            
            roiImage.ROI = gameSource.ButtonsArea;
            
            var newBotImage = new Image<Bgr, byte>(activeImage);
            newBotImage.Draw(gameSource.ButtonsArea, new Bgr(Color.Blue), 2);
            newBotImage.Draw(gameSource.RefreshButtonArea, new Bgr(Color.Blue), 2);
            
            var unitsRecognition = recognitionEngine.Recognize(roiImage);
            var visibleUnitsInShop = unitsRecognition.Text.SplitTrim(Environment.NewLine)
                .Where(IsValidPriceText)
                .Select((x, idx) => new UnitShopUnit() {Idx = idx, Price = x})
                .ToArray();
            AvailableUnits = IsValid(visibleUnitsInShop) ? visibleUnitsInShop : new UnitShopUnit[0];

            allText.AppendFormat($"Unit text:\n{unitsRecognition.Text}\n");
            
            roiImage.ROI = gameSource.RefreshButtonArea;
            var refreshButtonRecognition = recognitionEngine.Recognize(roiImage);
            allText.AppendFormat($"Refresh button:\n{refreshButtonRecognition.Text}\n");
            if (refreshButtonRecognition.Text == "00 00")
            {
                TimeLeftTillRefresh = TimeSpan.Zero;
            }
            else
            {
                TimeLeftTillRefresh = null;
            }

            BotImage = newBotImage.ToBitmap();
            RecognizedText = allText.ToString();
        }

        private bool IsValid(UnitShopUnit[] units)
        {
            return units.Length >= 1;
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