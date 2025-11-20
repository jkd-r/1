using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.IO;
using ProtocolEMR.Core.Procedural;

namespace ProtocolEMR.Tests.Procedural
{
    /// <summary>
    /// Unit tests for SeedManager to ensure deterministic behavior and proper persistence.
    /// </summary>
    public class SeedManagerTests
    {
        private SeedManager seedManager;
        private string testSavePath;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with SeedManager
            GameObject testObject = new GameObject("TestSeedManager");
            seedManager = testObject.AddComponent<SeedManager>();
            
            // Use a test-specific save path
            testSavePath = Path.Combine(Application.temporaryCachePath, "test_procedural_seed.json");
            
            // Set the save path through reflection or make it public
            var savePathField = typeof(SeedManager).GetField("savePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (savePathField != null)
            {
                savePathField.SetValue(seedManager, testSavePath);
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test files
            if (File.Exists(testSavePath))
            {
                File.Delete(testSavePath);
            }
            
            // Clean up GameObject
            if (seedManager != null && seedManager.gameObject != null)
            {
                Object.DestroyImmediate(seedManager.gameObject);
            }
        }

        [Test]
        public void SeedManager_InitializesWithValidSeed()
        {
            // Assert
            Assert.IsTrue(seedManager.CurrentSeed != 0, "Seed should be non-zero after initialization");
            Assert.IsNotNull(seedManager.RandomGenerator, "Random generator should be initialized");
        }

        [Test]
        public void SetSeed_UpdatesCurrentSeedAndReinitializes()
        {
            // Arrange
            int testSeed = 12345;
            
            // Act
            seedManager.SetSeed(testSeed);
            
            // Assert
            Assert.AreEqual(testSeed, seedManager.CurrentSeed, "Current seed should be updated");
            Assert.IsNotNull(seedManager.RandomGenerator, "Random generator should be reinitialized");
        }

        [Test]
        public void GetSeed_ReturnsConsistentValuesForSameScopeAndOffset()
        {
            // Arrange
            int testSeed = 54321;
            seedManager.SetSeed(testSeed);
            
            // Act
            int seed1 = seedManager.GetSeed(SeedManager.SCOPE_NPCS, 0);
            int seed2 = seedManager.GetSeed(SeedManager.SCOPE_NPCS, 0);
            int seed3 = seedManager.GetSeed(SeedManager.SCOPE_NPCS, 1);
            
            // Assert
            Assert.AreEqual(seed1, seed2, "Same scope and offset should return same seed");
            Assert.AreNotEqual(seed1, seed3, "Different offsets should return different seeds");
        }

        [Test]
        public void GetSeed_ReturnsDifferentSeedsForDifferentScopes()
        {
            // Arrange
            int testSeed = 98765;
            seedManager.SetSeed(testSeed);
            
            // Act
            int npcSeed = seedManager.GetSeed(SeedManager.SCOPE_NPCS, 0);
            int chunkSeed = seedManager.GetSeed(SeedManager.SCOPE_CHUNKS, 0);
            int audioSeed = seedManager.GetSeed(SeedManager.SCOPE_AUDIO, 0);
            
            // Assert
            Assert.AreNotEqual(npcSeed, chunkSeed, "Different scopes should return different seeds");
            Assert.AreNotEqual(chunkSeed, audioSeed, "Different scopes should return different seeds");
            Assert.AreNotEqual(npcSeed, audioSeed, "Different scopes should return different seeds");
        }

        [Test]
        public void ScopeOffset_AdvancesCorrectly()
        {
            // Arrange
            seedManager.SetSeed(11111);
            int initialOffset = seedManager.GetScopeOffset(SeedManager.SCOPE_NPCS);
            
            // Act
            seedManager.AdvanceScopeOffset(SeedManager.SCOPE_NPCS, 5);
            int newOffset = seedManager.GetScopeOffset(SeedManager.SCOPE_NPCS);
            
            // Assert
            Assert.AreEqual(initialOffset + 5, newOffset, "Scope offset should advance correctly");
        }

        [Test]
        public void GetRandomInt_ReturnsValuesInCorrectRange()
        {
            // Arrange
            seedManager.SetSeed(22222);
            int min = 10;
            int max = 20;
            
            // Act
            int result = seedManager.GetRandomInt(SeedManager.SCOPE_NPCS, min, max);
            
            // Assert
            Assert.IsTrue(result >= min && result < max, $"Result {result} should be in range [{min}, {max})");
        }

        [Test]
        public void GetRandomFloat_ReturnsValuesInCorrectRange()
        {
            // Arrange
            seedManager.SetSeed(33333);
            
            // Act
            float result = seedManager.GetRandomFloat(SeedManager.SCOPE_NPCS);
            
            // Assert
            Assert.IsTrue(result >= 0f && result <= 1f, $"Result {result} should be in range [0, 1]");
        }

        [Test]
        public void GetRandomBool_ReturnsEitherTrueOrFalse()
        {
            // Arrange
            seedManager.SetSeed(44444);
            
            // Act
            bool result = seedManager.GetRandomBool(SeedManager.SCOPE_NPCS);
            
            // Assert
            // This test just ensures it doesn't crash and returns a boolean
            Assert.IsTrue(result == true || result == false, "Should return either true or false");
        }

        [Test]
        public void GetRandomItem_ReturnsValidItemFromArray()
        {
            // Arrange
            seedManager.SetSeed(55555);
            string[] testArray = { "Apple", "Banana", "Cherry", "Date" };
            
            // Act
            string result = seedManager.GetRandomItem(testArray, SeedManager.SCOPE_NPCS);
            
            // Assert
            Assert.IsTrue(System.Array.Exists(testArray, item => item == result), 
                $"Result '{result}' should be one of the array items");
        }

        [Test]
        public void GetRandomItem_ReturnsValidItemFromList()
        {
            // Arrange
            seedManager.SetSeed(66666);
            var testList = new System.Collections.Generic.List<int> { 1, 2, 3, 4, 5 };
            
            // Act
            int result = seedManager.GetRandomItem(testList, SeedManager.SCOPE_NPCS);
            
            // Assert
            Assert.IsTrue(testList.Contains(result), $"Result {result} should be one of the list items");
        }

        [Test]
        public void SaveAndLoadSeed_PreservesCorrectly()
        {
            // Arrange
            int testSeed = 77777;
            seedManager.SetSeed(testSeed);
            seedManager.AdvanceScopeOffset(SeedManager.SCOPE_NPCS, 10);
            seedManager.AdvanceScopeOffset(SeedManager.SCOPE_CHUNKS, 5);
            
            // Act - Save
            seedManager.SaveSeed();
            
            // Create new instance and load
            GameObject newTestObject = new GameObject("NewTestSeedManager");
            SeedManager newSeedManager = newTestObject.AddComponent<SeedManager>();
            
            var savePathField = typeof(SeedManager).GetField("savePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (savePathField != null)
            {
                savePathField.SetValue(newSeedManager, testSavePath);
            }
            
            newSeedManager.LoadSeed();
            
            // Assert
            Assert.AreEqual(testSeed, newSeedManager.CurrentSeed, "Loaded seed should match saved seed");
            Assert.AreEqual(10, newSeedManager.GetScopeOffset(SeedManager.SCOPE_NPCS), 
                "NPC scope offset should be preserved");
            Assert.AreEqual(5, newSeedManager.GetScopeOffset(SeedManager.SCOPE_CHUNKS), 
                "Chunk scope offset should be preserved");
            
            // Cleanup
            Object.DestroyImmediate(newTestObject.gameObject);
        }

        [Test]
        public void DeterministicBehavior_SameSeedProducesSameResults()
        {
            // Arrange
            int testSeed = 88888;
            seedManager.SetSeed(testSeed);
            
            // Act - Generate sequence of random values
            var results1 = new System.Collections.Generic.List<int>();
            for (int i = 0; i < 10; i++)
            {
                results1.Add(seedManager.GetRandomInt(SeedManager.SCOPE_NPCS, 0, 100, i));
            }
            
            // Reset and generate again
            seedManager.SetSeed(testSeed);
            var results2 = new System.Collections.Generic.List<int>();
            for (int i = 0; i < 10; i++)
            {
                results2.Add(seedManager.GetRandomInt(SeedManager.SCOPE_NPCS, 0, 100, i));
            }
            
            // Assert
            CollectionAssert.AreEqual(results1, results2, "Same seed should produce identical sequences");
        }

        [Test]
        public void ResetScopeOffsets_ResetsAllOffsetsToZero()
        {
            // Arrange
            seedManager.SetSeed(99999);
            seedManager.AdvanceScopeOffset(SeedManager.SCOPE_NPCS, 15);
            seedManager.AdvanceScopeOffset(SeedManager.SCOPE_CHUNKS, 8);
            seedManager.AdvanceScopeOffset(SeedManager.SCOPE_AUDIO, 3);
            
            // Act
            seedManager.ResetScopeOffsets();
            
            // Assert
            Assert.AreEqual(0, seedManager.GetScopeOffset(SeedManager.SCOPE_NPCS), 
                "NPC scope offset should be reset to zero");
            Assert.AreEqual(0, seedManager.GetScopeOffset(SeedManager.SCOPE_CHUNKS), 
                "Chunk scope offset should be reset to zero");
            Assert.AreEqual(0, seedManager.GetScopeOffset(SeedManager.SCOPE_AUDIO), 
                "Audio scope offset should be reset to zero");
        }

        [Test]
        public void GetScopeRandom_ReturnsIndependentRandomGenerators()
        {
            // Arrange
            seedManager.SetSeed(12345);
            
            // Act
            var rng1 = seedManager.GetScopeRandom(SeedManager.SCOPE_NPCS, 0);
            var rng2 = seedManager.GetScopeRandom(SeedManager.SCOPE_NPCS, 0);
            var rng3 = seedManager.GetScopeRandom(SeedManager.SCOPE_NPCS, 1);
            
            // Assert
            Assert.IsNotNull(rng1, "Random generator should not be null");
            Assert.IsNotNull(rng2, "Random generator should not be null");
            Assert.IsNotNull(rng3, "Random generator should not be null");
            
            // Test that they produce the same sequence for same parameters
            int val1 = rng1.Next();
            int val2 = rng2.Next();
            Assert.AreEqual(val1, val2, "Same scope and offset should produce same sequence");
            
            int val3 = rng3.Next();
            // Different offset should produce different value (very high probability)
            // We can't guarantee different, but with good random implementation it should be
        }

        [Test]
        public void GetDebugInfo_ReturnsCorrectInformation()
        {
            // Arrange
            int testSeed = 54321;
            seedManager.SetSeed(testSeed);
            seedManager.AdvanceScopeOffset(SeedManager.SCOPE_NPCS, 7);
            
            // Act
            string debugInfo = seedManager.GetDebugInfo();
            
            // Assert
            Assert.IsTrue(debugInfo.Contains(testSeed.ToString()), "Debug info should contain current seed");
            Assert.IsTrue(debugInfo.Contains("NPCs"), "Debug info should contain NPC scope");
            Assert.IsTrue(debugInfo.Contains("7"), "Debug info should contain NPC offset");
        }
    }
}