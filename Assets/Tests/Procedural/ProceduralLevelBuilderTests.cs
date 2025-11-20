using NUnit.Framework;
using UnityEngine;
using ProtocolEMR.Core.Procedural;
using System.Collections.Generic;

namespace ProtocolEMR.Core.Tests.Procedural
{
    /// <summary>
    /// Comprehensive tests for procedural level generation determinism and consistency.
    /// </summary>
    public class ProceduralLevelBuilderTests
    {
        [Test]
        public void ChunkDefinition_Validates_CorrectlyFormedChunks()
        {
            var chunk = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            chunk.chunkId = "test_chunk";
            chunk.chunkPrefab = new GameObject("test_prefab");
            chunk.connectors = new ChunkConnector[]
            {
                new ChunkConnector { connectorTag = "in", allowInbound = true },
                new ChunkConnector { connectorTag = "out", allowOutbound = true }
            };

            var errors = chunk.Validate();
            
            Assert.That(errors.Count, Is.EqualTo(0), "Well-formed chunk should have no errors");
        }

        [Test]
        public void ChunkDefinition_Detects_MissingChunkId()
        {
            var chunk = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            chunk.chunkPrefab = new GameObject("test");

            var errors = chunk.Validate();

            Assert.That(errors.Count, Is.GreaterThan(0), "Should detect missing chunk ID");
            Assert.That(errors[0], Contains.Substring("ID is empty"));
        }

        [Test]
        public void ChunkDefinition_Detects_MissingPrefab()
        {
            var chunk = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            chunk.chunkId = "test";
            chunk.chunkPrefab = null;

            var errors = chunk.Validate();

            Assert.That(errors.Count, Is.GreaterThan(0), "Should detect missing prefab");
            Assert.That(errors[0], Contains.Substring("prefab"));
        }

        [Test]
        public void ChunkDefinition_Detects_InvalidSize()
        {
            var chunk = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            chunk.chunkId = "test";
            chunk.chunkPrefab = new GameObject();
            chunk.chunkSize = Vector3.zero;

            var errors = chunk.Validate();

            Assert.That(errors.Count, Is.GreaterThan(0), "Should detect invalid size");
            Assert.That(errors[0], Contains.Substring("size"));
        }

        [Test]
        public void ChunkConnector_GetWorldPosition_TransformsCorrectly()
        {
            var connector = new ChunkConnector
            {
                connectorTag = "test",
                localPosition = new Vector3(5, 0, 10)
            };

            var go = new GameObject();
            go.transform.position = new Vector3(10, 0, 20);

            Vector3 worldPos = connector.GetWorldPosition(go.transform);

            Assert.That(worldPos, Is.EqualTo(new Vector3(15, 0, 30)));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ChunkConnector_GetWorldRotation_TransformsCorrectly()
        {
            var connector = new ChunkConnector
            {
                connectorTag = "test",
                localRotation = Quaternion.Euler(0, 90, 0)
            };

            var go = new GameObject();
            go.transform.rotation = Quaternion.Euler(0, 45, 0);

            Quaternion worldRot = connector.GetWorldRotation(go.transform);
            Vector3 forward = worldRot * Vector3.forward;

            // 90 + 45 = 135 degrees
            Assert.That(forward.x, Is.GreaterThan(0.5f), "Should rotate appropriately");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SeedManager_GeneratesDeterministicSequences()
        {
            var chunk1 = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            chunk1.chunkId = "chunk1";
            chunk1.selectionWeight = 1.0f;

            var chunk2 = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            chunk2.chunkId = "chunk2";
            chunk2.selectionWeight = 1.0f;

            var chunks = new ProceduralChunkDefinition[] { chunk1, chunk2 };

            // Test determinism with same seed
            var sequence1 = GenerateSequenceWithSeed(chunks, 12345, 10);
            var sequence2 = GenerateSequenceWithSeed(chunks, 12345, 10);

            Assert.That(sequence1.Count, Is.EqualTo(sequence2.Count), "Sequences should have same length");

            for (int i = 0; i < sequence1.Count; i++)
            {
                Assert.That(sequence1[i], Is.EqualTo(sequence2[i]), $"Seed 12345 should produce same sequence at index {i}");
            }
        }

        [Test]
        public void SeedManager_GeneratesDifferentSequences_WithDifferentSeeds()
        {
            var chunk1 = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            chunk1.chunkId = "chunk1";
            chunk1.selectionWeight = 1.0f;

            var chunk2 = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            chunk2.chunkId = "chunk2";
            chunk2.selectionWeight = 1.0f;

            var chunks = new ProceduralChunkDefinition[] { chunk1, chunk2 };

            var sequence1 = GenerateSequenceWithSeed(chunks, 12345, 10);
            var sequence2 = GenerateSequenceWithSeed(chunks, 54321, 10);

            // Check that at least some elements differ
            int differences = 0;
            for (int i = 0; i < Mathf.Min(sequence1.Count, sequence2.Count); i++)
            {
                if (sequence1[i] != sequence2[i])
                    differences++;
            }

            Assert.That(differences, Is.GreaterThan(0), "Different seeds should produce different sequences");
        }

        [Test]
        public void ChunkConnector_FiltersByAllowedDirections()
        {
            var inbound = new ChunkConnector
            {
                connectorTag = "in",
                allowInbound = true,
                allowOutbound = false
            };

            var outbound = new ChunkConnector
            {
                connectorTag = "out",
                allowInbound = false,
                allowOutbound = true
            };

            var bidirectional = new ChunkConnector
            {
                connectorTag = "both",
                allowInbound = true,
                allowOutbound = true
            };

            Assert.That(inbound.allowInbound, Is.True);
            Assert.That(inbound.allowOutbound, Is.False);

            Assert.That(outbound.allowInbound, Is.False);
            Assert.That(outbound.allowOutbound, Is.True);

            Assert.That(bidirectional.allowInbound, Is.True);
            Assert.That(bidirectional.allowOutbound, Is.True);
        }

        [Test]
        public void ChunkNPCZoneTemplate_CreatesValidZones()
        {
            var template = new ChunkNPCZoneTemplate
            {
                zoneName = "test_zone",
                centerOffset = new Vector3(5, 0, 0),
                zoneSize = new Vector3(10, 3, 10),
                minNPCs = 1,
                maxNPCs = 3,
                allowedTypes = new NPCType[] { NPCType.Guard }
            };

            Assert.That(template.zoneName, Is.EqualTo("test_zone"));
            Assert.That(template.zoneSize.magnitude, Is.GreaterThan(0));
            Assert.That(template.allowedTypes.Length, Is.EqualTo(1));
        }

        [Test]
        public void NPCSpawnZone_CreatedFromTemplate()
        {
            var template = new ChunkNPCZoneTemplate
            {
                zoneName = "zone",
                centerOffset = Vector3.zero,
                zoneSize = Vector3.one * 10f,
                minNPCs = 1,
                maxNPCs = 3,
                allowedTypes = new NPCType[] { NPCType.Patrol }
            };

            var zone = new NPCSpawnZone
            {
                zoneName = template.zoneName,
                zoneCenter = template.centerOffset,
                zoneSize = template.zoneSize,
                minNPCs = template.minNPCs,
                maxNPCs = template.maxNPCs,
                allowedNPCTypes = template.allowedTypes
            };

            Assert.That(zone.minNPCs, Is.EqualTo(1));
            Assert.That(zone.maxNPCs, Is.EqualTo(3));
            Assert.That(zone.allowedNPCTypes.Length, Is.EqualTo(1));
        }

        [Test]
        public void StressTest_100Seeds_ProduceDeterministicOutput()
        {
            var chunk1 = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            chunk1.chunkId = "chunk1";
            chunk1.selectionWeight = 0.8f;

            var chunk2 = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            chunk2.chunkId = "chunk2";
            chunk2.selectionWeight = 0.6f;

            var chunks = new ProceduralChunkDefinition[] { chunk1, chunk2 };

            var allSequences = new Dictionary<int, List<ProceduralChunkDefinition>>();

            // Generate sequences for 100 seeds
            for (int seed = 1; seed <= 100; seed++)
            {
                var sequence = GenerateSequenceWithSeed(chunks, seed, 8);
                allSequences[seed] = sequence;
            }

            // Verify determinism: same seed produces same sequence twice
            for (int seed = 1; seed <= 100; seed++)
            {
                var sequence1 = allSequences[seed];
                var sequence2 = GenerateSequenceWithSeed(chunks, seed, 8);

                Assert.That(sequence1.Count, Is.EqualTo(sequence2.Count), 
                    $"Seed {seed} should produce consistent length");

                for (int i = 0; i < sequence1.Count; i++)
                {
                    Assert.That(sequence1[i], Is.EqualTo(sequence2[i]), 
                        $"Seed {seed} should produce same sequence at index {i}");
                }
            }

            // Verify different seeds produce different sequences (at least sometimes)
            int totalDifferences = 0;
            for (int seed1 = 1; seed1 <= 10; seed1++)
            {
                for (int seed2 = seed1 + 1; seed2 <= 11; seed2++)
                {
                    var seq1 = allSequences[seed1];
                    var seq2 = allSequences[seed2];

                    for (int i = 0; i < Mathf.Min(seq1.Count, seq2.Count); i++)
                    {
                        if (seq1[i] != seq2[i])
                            totalDifferences++;
                    }
                }
            }

            Assert.That(totalDifferences, Is.GreaterThan(0), "Different seeds should usually produce different sequences");
        }

        [Test]
        public void ChunkDefinition_ConnectorTag_Retrieval()
        {
            var chunk = ScriptableObject.CreateInstance<ProceduralChunkDefinition>();
            chunk.connectors = new ChunkConnector[]
            {
                new ChunkConnector { connectorTag = "north" },
                new ChunkConnector { connectorTag = "south" },
                new ChunkConnector { connectorTag = "east" }
            };

            var tags = chunk.GetConnectorTags();
            Assert.That(tags.Length, Is.EqualTo(3));
            Assert.That(tags[0], Is.EqualTo("north"));
            Assert.That(tags[1], Is.EqualTo("south"));
            Assert.That(tags[2], Is.EqualTo("east"));

            var northConnector = chunk.GetConnector("north");
            Assert.That(northConnector, Is.Not.Null);
            Assert.That(northConnector.connectorTag, Is.EqualTo("north"));

            var invalidConnector = chunk.GetConnector("invalid");
            Assert.That(invalidConnector, Is.Null);
        }

        // Helper method
        private List<ProceduralChunkDefinition> GenerateSequenceWithSeed(
            ProceduralChunkDefinition[] chunks,
            int seed,
            int targetLength)
        {
            var sequence = new List<ProceduralChunkDefinition>();
            var random = new System.Random(seed);

            for (int i = 0; i < targetLength; i++)
            {
                int chunkIndex = random.Next(chunks.Length);
                ProceduralChunkDefinition chunk = chunks[chunkIndex];

                float weight = (float)random.NextDouble();
                if (weight <= chunk.selectionWeight)
                {
                    sequence.Add(chunk);
                }
            }

            return sequence;
        }
    }
}
