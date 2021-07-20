using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Content;
using Beamable.Editor.Content;
using Beamable.Editor.Tests.Beamable.Content.ContentIOTests;
using Beamable.Platform.SDK;
using Beamable.Platform.Tests;
using Beamable.Tests;
using NUnit.Framework;
using UnityEngine.TestTools;
using Manifest = Beamable.Editor.Content.Manifest;

namespace Beamable.Editor.Tests.Beamable.Content.ContentPublisherTests
{
   public class CreatePublishSetTests
   {
      private ContentPublisher _publisher;
      private MockContentIO _mockContentIo;
      private IEnumerable<ContentObject> _content;
      private List<ContentManifestReference> _serverContent;
      private MockPlatformRequester _requester;

      [SetUp]
      public void Init()
      {
         _content = new List<ContentObject>() { };
         _requester = new MockPlatformRequester();
         _serverContent = new List<ContentManifestReference>();
         _mockContentIo = new MockContentIO();
         _mockContentIo.FetchManifestResult = () => Promise<Editor.Content.Manifest>.Successful(new Editor.Content.Manifest(_serverContent));
         _mockContentIo.FindAllResult = () => _content;

         _publisher = new ContentPublisher(_requester, _mockContentIo);
      }

      [UnityTest]
      public IEnumerator ReturnsTheFetchedManifest()
      {
         yield return _publisher.CreatePublishSet().Then(set =>
         {
            Assert.IsNotNull(set.ServerManifest);
         }).AsYield();
      }

      [UnityTest]
      public IEnumerator ReturnsAdditions()
      {
         _content = new List<ContentObject> { ContentObject.Make<ExampleContent>("test") };

         yield return _publisher.CreatePublishSet().Then(set =>
         {
            Assert.AreEqual(1, set.ToAdd.Count);
         }).AsYield();
      }

      [UnityTest]
      public IEnumerator ReturnsModifications()
      {
         var someNewValue = 123;
         var modifiedContent = ContentObject.Make<ExampleContent>("test");
         modifiedContent.Value = someNewValue;

         _serverContent.Add(new ContentManifestReference()
         {
            id = modifiedContent.Id,
            checksum = "olddata"
         });

         _content = new List<ContentObject> { modifiedContent };

         yield return _publisher.CreatePublishSet().Then(set =>
         {
            Assert.AreEqual(1, set.ToModify.Count);
            Assert.AreEqual(someNewValue, ((ExampleContent) set.ToModify.First()).Value);
         }).AsYield();
      }

      [UnityTest]
      public IEnumerator ReturnsDeletions()
      {
         var id = "example.old";
         _serverContent.Add(new ContentManifestReference
         {
            id = id,
            checksum = "olddata"
         });
         yield return _publisher.CreatePublishSet().Then(set =>
         {
            Assert.AreEqual(1, set.ToDelete.Count);
            Assert.AreEqual(id, set.ToDelete.First());
         }).AsYield();
      }
   }
}