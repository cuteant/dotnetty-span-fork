﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using Xunit;

namespace DotNetty.Buffers.Tests
{
    public class Reader_ReadTo
    {
        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void TryReadTo_Span(bool advancePastDelimiter, bool useEscapeOverload)
        {
            ReadOnlySequence<byte> bytes = BufferFactory.Create(new byte[][] {
                new byte[] { 0 },
                new byte[] { 1, 2 },
                new byte[] { },
                new byte[] { 3, 4, 5, 6 }
            });

            ByteBufferReader reader = new ByteBufferReader(bytes);

            // Read to 0-5
            for (byte i = 0; i < bytes.Length - 1; i++)
            {
                ByteBufferReader copy = reader;

                // Can read to the first integer (0-5)
                Assert.True(
                    useEscapeOverload
                        ? copy.TryReadTo(out ReadOnlySpan<byte> span, i, 255, advancePastDelimiter)
                        : copy.TryReadTo(out span, i, advancePastDelimiter));

                // Should never have a null Position object
                Assert.NotNull(copy.Position.GetObject());

                // Should be able to then read to 6
                Assert.True(
                    useEscapeOverload
                        ? copy.TryReadTo(out span, 6, 255, advancePastDelimiter)
                        : copy.TryReadTo(out span, 6, advancePastDelimiter));

                Assert.NotNull(copy.Position.GetObject());

                // If we didn't advance, we should still be able to read to 6
                Assert.Equal(!advancePastDelimiter,
                    useEscapeOverload
                        ? copy.TryReadTo(out span, 6, 255, advancePastDelimiter)
                        : copy.TryReadTo(out span, 6, advancePastDelimiter));
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void TryReadTo_Sequence(bool advancePastDelimiter, bool useEscapeOverload)
        {
            ReadOnlySequence<byte> bytes = BufferFactory.Create(new byte[][] {
                new byte[] { 0 },
                new byte[] { 1, 2 },
                new byte[] { },
                new byte[] { 3, 4, 5, 6 }
            });

            ByteBufferReader reader = new ByteBufferReader(bytes);

            // Read to 0-5
            for (byte i = 0; i < bytes.Length - 1; i++)
            {
                ByteBufferReader copy = reader;

                // Can read to the first integer (0-5)
                Assert.True(
                    useEscapeOverload
                        ? copy.TryReadTo(out ReadOnlySequence<byte> sequence, i, 255, advancePastDelimiter)
                        : copy.TryReadTo(out sequence, i, advancePastDelimiter));

                // Should never have a null Position object
                Assert.NotNull(copy.Position.GetObject());
                var enumerator = sequence.GetEnumerator();
                while (enumerator.MoveNext()) ;

                // Should be able to read to final 6
                Assert.True(
                    useEscapeOverload
                        ? copy.TryReadTo(out sequence, 6, 255, advancePastDelimiter)
                        : copy.TryReadTo(out sequence, 6, advancePastDelimiter));

                Assert.NotNull(copy.Position.GetObject());
                enumerator = sequence.GetEnumerator();
                while (enumerator.MoveNext()) ;

                // If we didn't advance, we should still be able to read to 6
                Assert.Equal(!advancePastDelimiter,
                    useEscapeOverload
                        ? copy.TryReadTo(out sequence, 6, 255, advancePastDelimiter)
                        : copy.TryReadTo(out sequence, 6, advancePastDelimiter));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TryReadToSpan_Sequence(bool advancePastDelimiter)
        {
            ReadOnlySequence<byte> bytes = BufferFactory.Create(new byte[][] {
                new byte[] { 0, 0 },
                new byte[] { 1, 1, 2, 2 },
                new byte[] { },
                new byte[] { 3, 3, 4, 4, 5, 5, 6, 6 }
            });

            ByteBufferReader reader = new ByteBufferReader(bytes);
            for (byte i = 0; i < bytes.Length / 2 - 1; i++)
            {
                byte[] expected = new byte[i * 2 + 1];
                for (int j = 0; j < expected.Length - 1; j++)
                {
                    expected[j] = (byte)(j / 2);
                }
                expected[i * 2] = i;
                ReadOnlySpan<byte> searchFor = new byte []{ i, (byte)(i + 1) };
                ByteBufferReader copy = reader;
                Assert.True(copy.TryReadTo(out ReadOnlySequence<byte> seq, searchFor, advancePastDelimiter));
                Assert.True(seq.ToArray().AsSpan().SequenceEqual(expected));
            }

            bytes = BufferFactory.Create(new byte[][] {
                new byte[] { 47, 42, 66, 32, 42, 32, 66, 42, 47 }   // /*b * b*/
            });

            reader = new ByteBufferReader(bytes);
            Assert.True(reader.TryReadTo(out ReadOnlySequence<byte> sequence, new byte[] { 42, 47 }, advancePastDelimiter));    //  */
            Assert.True(sequence.ToArray().AsSpan().SequenceEqual(new byte[] { 47, 42, 66, 32, 42, 32, 66 }));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TryReadTo_NotFound_Span(bool advancePastDelimiter)
        {
            ReadOnlySequence<byte> bytes = BufferFactory.Create(new byte[][] {
                new byte[] { 1 },
                new byte[] { 2, 3, 255 }
            });

            ByteBufferReader reader = new ByteBufferReader(bytes);
            reader.Advance(4);
            Assert.False(reader.TryReadTo(out ReadOnlySpan<byte> span, 255, 0, advancePastDelimiter));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TryReadTo_NotFound_Sequence(bool advancePastDelimiter)
        {
            ReadOnlySequence<byte> bytes = BufferFactory.Create(new byte[][] {
                new byte[] { 1 },
                new byte[] { 2, 3, 255 }
            });

            ByteBufferReader reader = new ByteBufferReader(bytes);
            reader.Advance(4);
            Assert.False(reader.TryReadTo(out ReadOnlySequence<byte> span, 255, 0, advancePastDelimiter));
        }
    }
}
