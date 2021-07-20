using Beamable.Editor.Content.Models;
using Beamable.Editor.Content;
using UnityEngine;
using Beamable.Editor.UI.Buss.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Platform.SDK;
using Beamable.Editor.UI.Components;
using UnityEditor;
using Debug = UnityEngine.Debug;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
   public class DownloadContentVisualElement : ContentManagerComponent
   {
      public Promise<DownloadSummary> Model { get; set; }

      // Proceed a refresh for content manager when downloads succeeded
      public event Action OnRefreshContentManager;
      public event Action OnCancelled;
      public event Action OnClosed;
      public event Action<DownloadSummary, HandleContentProgress, HandleDownloadFinished> OnDownloadStarted;

      private Button _cancelBtn;
      private LoadingBarVisualElement _loadingBar;
      private PrimaryButtonVisualElement _downloadBtn;

      private List<ContentPopupLinkVisualElement> _contentElements = new List<ContentPopupLinkVisualElement>();
      private ListView _modifiedList;
      private ListView _addList;
      private bool _allDownloadsComplete;
      private Label _messageLabel;

      public DownloadContentVisualElement() : base(nameof(DownloadContentVisualElement))
      {
      }

      public override void Refresh()
      {
         base.Refresh();
         var mainElement = Root.Q<VisualElement>("mainVisualElement");
         var loadingBlocker = Root.Q<LoadingIndicatorVisualElement>();
         var promise = Model.Then(summary =>
         {

            _messageLabel = Root.Q<Label>("message");
            _messageLabel.text = ContentManagerConstants.DownloadMessageText;
            _messageLabel.AddTextWrapStyle();

            var overrideCount = Root.Q<CountVisualElement>("overrideCount");
            overrideCount.SetValue(summary.Overwrites.Count());
            var addInCount = Root.Q<CountVisualElement>("addInCount");
            addInCount.SetValue(summary.Additions.Count());

            _cancelBtn = Root.Q<Button>("cancelBtn");
            _cancelBtn.clickable.clicked += CancelButton_OnClicked;

            _downloadBtn = Root.Q<PrimaryButtonVisualElement>("downloadBtn");
            _downloadBtn.Button.clickable.clicked += DownloadButton_OnClicked;

            _loadingBar = Root.Q<LoadingBarVisualElement>();

            var noDownloadLabel = Root.Q<Label>("noDownloadLbl");
            noDownloadLabel.text = ContentManagerConstants.DownloadNoDataText;
            noDownloadLabel.AddTextWrapStyle();
            if (summary.TotalDownloadEntries > 0)
            {
               noDownloadLabel.parent.Remove(noDownloadLabel);
            }

            // TODO show preview of download content.
            var modifiedFold = Root.Q<Foldout>("overwriteFoldout");
            modifiedFold.text = "Overwrites";
            var modifiedSource = new List<ContentDownloadEntryDescriptor>();
            _modifiedList = new ListView
            {
               itemHeight = 24,
               itemsSource = modifiedSource,
               makeItem = MakeElement,
               bindItem = CreateBinder(modifiedSource)
            };
            modifiedFold.contentContainer.Add(_modifiedList);

            var additionFold = Root.Q<Foldout>("addFoldout");
            additionFold.text = "Additions";
            var addSource = new List<ContentDownloadEntryDescriptor>();
            _addList = new ListView
            {
               itemHeight = 24,
               itemsSource = addSource,
               makeItem = MakeElement,
               bindItem = CreateBinder(addSource)
            };
            additionFold.contentContainer.Add(_addList);

            if (summary.AnyOverwrites)
            {
               modifiedSource.AddRange(summary.Overwrites);
               modifiedFold.Q<ListView>().style.height = _modifiedList.itemHeight * summary.Overwrites.Count();
               _modifiedList.Refresh();
            }
            else
            {
               modifiedFold.parent.Remove(modifiedFold);
            }

            if (summary.AnyAdditions)
            {
               addSource.AddRange(summary.Additions);
               additionFold.Q<ListView>().style.height = _addList.itemHeight * summary.Additions.Count();
               _addList.Refresh();

            }
            else
            {
               additionFold.parent.Remove(additionFold);
            }
         });
         loadingBlocker.SetPromise(promise, mainElement).SetText(ContentManagerConstants.DownloadLoadText);
      }

      private ContentPopupLinkVisualElement MakeElement()
      {
         var contentPopupLinkVisualElement = new ContentPopupLinkVisualElement();
         _contentElements.Add(contentPopupLinkVisualElement);
         // return new ContentPopupLinkVisualElement();
         return contentPopupLinkVisualElement;
      }

      private Action<VisualElement, int> CreateBinder(List<ContentDownloadEntryDescriptor> source)
      {
         return (elem, index) =>
         {
            var link = elem as ContentPopupLinkVisualElement;
            link.Model = source[index];
            if (_allDownloadsComplete)
            {
               link.MarkChecked();
            }
            link.Refresh();
         };
      }

      private void CancelButton_OnClicked()
      {
         // TODO Be smarter about how we cancel the download.
         OnCancelled?.Invoke();
         OnRefreshContentManager?.Invoke();
      }

      private void DownloadButton_OnClicked()
      {
         if (_allDownloadsComplete)
         {
            OnClosed?.Invoke();
            OnRefreshContentManager?.Invoke();
         }
         else
         {
            HandleDownload();
         }
      }


      private void HandleDownload()
      {
         var lastProcessed = 0;
         OnDownloadStarted?.Invoke(Model.GetResult(), (progress, processed, total) =>
         {
            _loadingBar.Value = progress;
            //Mark element as checked
            for (var i = lastProcessed; i <processed; i++ )
            {
               var contentElement = _contentElements[i];
               contentElement.MarkChecked();
            }
            lastProcessed = processed;

         }, finalPromise =>
            {
               _downloadBtn.Load(finalPromise);

               finalPromise.Then(_ =>
               {

                  _messageLabel.text = ContentManagerConstants.DownloadCompleteText;
                     _downloadBtn.SetText("Okay");
                     _allDownloadsComplete = true;
                     _loadingBar.Value = 1;

                     // Mark all as checked
                     this.MarkDirtyRepaint();
                     EditorApplication.delayCall += () =>
                     {
                        foreach (var contentElement in _contentElements)
                           contentElement.MarkChecked();
                        this.MarkDirtyRepaint();
                     };

               }).Error(_ =>
                  {
                     _loadingBar.Value = 1;
                     // TODO make this error reporting better.
                     EditorApplication.delayCall += () =>
                     {
                        EditorUtility.DisplayDialog("Download Failed", "See console for errors.", "OK");
                        OnClosed?.Invoke();
                     };
                  });
            });
      }
   }
}