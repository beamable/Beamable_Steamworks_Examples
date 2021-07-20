
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Editor.Content.Models;
using Beamable.Editor.UI.Components;
using UnityEditor;
using UnityEngine;
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
   public class ValidateContentVisualElement : ContentManagerComponent
   {

      public ContentDataModel DataModel { get; set; }

      public event Action OnCancelled;
      public event Action OnClosed;
      private LoadingBarVisualElement _progressBar;
      private Label _messageLbl;
      private Button _detailButton;

      private CountVisualElement _invalidContentCountVisualElement;
      private CountVisualElement _totalErrorCountVisualElement;

      private ListView _listView;
      private IList _listSource = new List<ContentExceptionCollection>();
      private VisualElement _errorContainer;
      private PrimaryButtonVisualElement _okayButton;
      private Button _cancelButton;

      private Label _emptyMessageLabel;
      private Promise<Unit> _completePromise = new Promise<Unit>();
      private Dictionary<ContentObject, Action<List<ContentException>>> _validationHandlers = new Dictionary<ContentObject, Action<List<ContentException>>>();

      public ValidateContentVisualElement() : base(nameof(ValidateContentVisualElement))
      {
      }

      public override void Refresh()
      {
         base.Refresh();
         _progressBar = Root.Q<LoadingBarVisualElement>();
         _progressBar.Value = 0;

         _messageLbl = Root.Q<Label>("message");
         _messageLbl.text = ContentManagerConstants.ValidateStartMessage;

         _invalidContentCountVisualElement = Root.Q<CountVisualElement>("errorObjectCount");
         _totalErrorCountVisualElement = Root.Q<CountVisualElement>("errorCount");
         UpdateErrorCount();

         _okayButton = Root.Q<PrimaryButtonVisualElement>("okayBtn");
         _okayButton.Button.text = ContentManagerConstants.ValidateButtonStartText;
         _okayButton.Button.clickable.clicked += OkayButton_OnClicked;
         _okayButton.Disable();
         _okayButton.Load(_completePromise);

         _cancelButton = Root.Q<Button>("cancelBtn");
         _cancelButton.clickable.clicked += CancelButton_OnClicked;

         _errorContainer = Root.Q<VisualElement>("errorContainer");
         _listView = new ListView(_listSource, 24, CreateListItem, BindListItem);
         _listView.BeamableOnSelectionsChanged(enumerableSelection =>
         {
            var selections = enumerableSelection.ToList();
            if (selections.Count != 1) return;

            var errs = selections[0] as ContentExceptionCollection;
            Selection.SetActiveObjectWithContext(errs.Content as ScriptableObject, null);
         });
         _emptyMessageLabel = new Label("No validation errors yet.");
         _emptyMessageLabel.name = "emptyMessage";
         _errorContainer.Add(_listView);
         _errorContainer.Add(_emptyMessageLabel);

      }

      private void UpdateErrorCount()
      {

         var totalContent = 0;
         int totalErrorCount = 0;
         foreach (var exceptionObj in _listSource)
         {
            var count = AccumulateErrorTotals(exceptionObj);
            totalContent += (count > 0 ? 1 : 0);
            totalErrorCount += count;
         }
         _invalidContentCountVisualElement.SetValue(totalContent);
         _totalErrorCountVisualElement.SetValue(totalErrorCount);
      }

      private int AccumulateErrorTotals(object elem)
      {
         var exceptionCollection = elem as ContentExceptionCollection;
         return exceptionCollection?.Exceptions?.Count() ?? 0;
      }

      private void OkayButton_OnClicked()
      {
         if (_listSource.Count != 0)
         {
            DataModel.SetFilter("valid:n");
         }

         OnClosed?.Invoke();
      }

      private void CancelButton_OnClicked()
      {
         OnCancelled?.Invoke();
      }

      ContentValidationErrorVisualElement CreateListItem()
      {
         return new ContentValidationErrorVisualElement();
      }

      void BindListItem(VisualElement element, int index)
      {
         var view = element as ContentValidationErrorVisualElement;
         var data = _listSource[index] as ContentExceptionCollection;

         if (view == null || data == null)
         {
            Debug.LogWarning("Validation ListView binding content incorrectly.");
            return;
         }

         if (!data.AnyExceptions)
         {
            view.AddToClassList("noErrors");
         }
         else
         {
            view.RemoveFromClassList("noErrors");
         }

         view.ExceptionCollection = data;
         view.Refresh();
      }

      public void SetProgress(float progress, int processed, int total)
      {
         // TODO: can the cb be put on the delayCall?
         EditorApplication.delayCall += () =>
         {
            if (processed < total)
            {
               _messageLbl.text = $"{ContentManagerConstants.ValidateProgressMessage} {processed}/{total}";
            }
         };
         _progressBar.Value = progress;
      }


      public void HandleValidationErrors(ContentExceptionCollection errors)
      {
         _emptyMessageLabel.AddToClassList("hidden");
         _progressBar.AddToClassList("failed");
         _listSource.Add(errors);

         _totalErrorCountVisualElement.SetValue(_totalErrorCountVisualElement.Value + errors.Exceptions.Count);
         _invalidContentCountVisualElement.SetValue(_listSource.Count);

         _listView.Refresh();
      }

      public override void OnDetach()
      {
         base.OnDetach();
         foreach (var elem in _listSource)
         {
            var err = elem as ContentExceptionCollection;
            var content = err?.Content as ContentObject;
            if (content != null && _validationHandlers.TryGetValue(content, out var handler))
            {
               content.OnValidationChanged -= handler;
            }
         }
      }


      public void HandleFinished()
      {
         _completePromise.CompleteSuccess(PromiseBase.Unit);
         _emptyMessageLabel.AddToClassList("hidden");
         var areErrors = _listSource.Count > 0;
         if (areErrors)
         {
            _messageLbl.text = ContentManagerConstants.ValidationFailureMessage;
            _messageLbl.AddToClassList("failed");
            _okayButton.Button.text = ContentManagerConstants.ValidateButtonDoneWithErrorsText;


            foreach (var elem in _listSource)
            {
               var err = elem as ContentExceptionCollection;
               var content = err?.Content as ContentObject;
               if (content != null)
               {
                  var handler = new Action<List<ContentException>>(exceptions =>
                  {
                     err.Exceptions = exceptions;
                     _listView.Refresh();
                     UpdateErrorCount();
                  });
                  _validationHandlers.Add(content, handler);
                  content.OnValidationChanged += handler;
               }
            }

         }
         else
         {
            _messageLbl.text = ContentManagerConstants.ValidationCompleteMessage;
            _okayButton.Button.text = ContentManagerConstants.ValidateButtonDoneWithoutErrorsText;
         }


         UpdateErrorCount();

         _okayButton.Enable();

      }
   }
}