﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trady.Analysis.Helper;
using Trady.Analysis.Infrastructure;
using Trady.Core;

namespace Trady.Analysis.Pattern.Candlestick
{
    public class LongUpperShadow<TInput, TOutput> : AnalyzableBase<TInput, (decimal Open, decimal High, decimal Close), bool?, TOutput>
    {
        public LongUpperShadow(IEnumerable<TInput> inputs, Func<TInput, (decimal Open, decimal High, decimal Close)> inputMapper, Func<TInput, bool?, TOutput> outputMapper, int periodCount = 20, decimal threshold = 0.75m) : base(inputs, inputMapper, outputMapper)
        {
			    PeriodCount = periodCount;
			    Threshold = threshold;
		}

        public int PeriodCount { get; }
        public decimal Threshold { get; }

        protected override bool? ComputeByIndexImpl(IEnumerable<(decimal Open, decimal High, decimal Close)> mappedInputs, int index)
        {
            var upperShadows = mappedInputs.Select(i => i.High - Math.Max(i.Open, i.Close));
			return upperShadows.ElementAt(index) >= upperShadows.Percentile(PeriodCount, index, Threshold);        
        }
    }

    public class LongUpperShadowByTuple : LongUpperShadow<(decimal Open, decimal High, decimal Close), bool?>
    {
        public LongUpperShadowByTuple(IEnumerable<(decimal Open, decimal High, decimal Close)> inputs, int periodCount = 20, decimal threshold = 0.75M) 
            : base(inputs, i => i, (i, otm) => otm, periodCount, threshold)
        {
        }
    }

    public class LongUpperShadow : LongUpperShadow<Candle, AnalyzableTick<bool?>>
    {
        public LongUpperShadow(IEnumerable<Candle> inputs, int periodCount = 20, decimal threshold = 0.75M) 
            : base(inputs, i => (i.Open, i.High, i.Close), (i, otm) => new AnalyzableTick<bool?>(i.DateTime, otm), periodCount, threshold)
        {
        }
    }
}