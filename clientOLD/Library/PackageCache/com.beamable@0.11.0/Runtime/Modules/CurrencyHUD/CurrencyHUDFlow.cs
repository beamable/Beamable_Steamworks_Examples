﻿using System.Collections;
using Beamable.Common.Inventory;
using Beamable.Content;
using Beamable.Coroutines;
using Beamable.Inventory;
using Beamable.Service;
using Beamable.UI.Scripts;
using TMPro;

using UnityEngine;

using UnityEngine.UI;

using UnityEngine.AddressableAssets;



namespace Beamable.CurrencyHUD

{


   [HelpURL(BeamableConstants.URL_FEATURE_CURRENCY_HUD)]
   public class CurrencyHUDFlow : MonoBehaviour

    {

        public CurrencyRef content;

        public Canvas canvas;

        public RawImage img;

        public TextMeshProUGUI txtAmount;

        private long targetAmount = 0;

        private long currentAmount = 0;



        void Awake()

        {

            canvas.enabled = false;

        }

        private async void Start()
        {
            var de = await API.Instance;
            de.InventoryService.Subscribe(content.Id, view =>
            {
                view.currencies.TryGetValue(content.Id, out targetAmount);
                ServiceManager.Resolve<CoroutineService>().StartCoroutine(DisplayCurrency());
            });
            var currency = await content.Resolve();
            img.texture = await currency.icon.LoadTexture();
            canvas.enabled = true;
        }

        private IEnumerator DisplayCurrency()
        {
            long deltaTotal = targetAmount - currentAmount;
            long deltaStep = deltaTotal / 50;

            if (deltaStep == 0)
            {
                deltaStep = deltaTotal < 0 ? -1 : 1;
            }

            while (currentAmount != targetAmount)

            {

                currentAmount += deltaStep;

                if (deltaTotal > 0 && currentAmount > targetAmount)

                {

                    currentAmount = targetAmount;

                }

                else if (deltaTotal < 0 && currentAmount < targetAmount)

                {

                    currentAmount = targetAmount;

                }



                txtAmount.text = currentAmount.ToString();

                yield return new WaitForSeconds(0.02f);

            }

        }

    }

}