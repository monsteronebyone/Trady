﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trady.Analysis.Infrastructure;
using Trady.Core;

namespace Trady.Analysis.Pattern.Candlestick
{
    public class EveningDojiStar<TInput, TOutput> : AnalyzableBase<TInput, (decimal Open, decimal High, decimal Low, decimal Close), bool?, TOutput>
    {
        UpTrendByTuple _upTrend;
        BullishLongDayByTuple _bullishLongDay;
        DojiByTuple _doji;
        BearishLongDayByTuple _bearishLongDay;

        public EveningDojiStar(IEnumerable<TInput> inputs, Func<TInput, (decimal Open, decimal High, decimal Low, decimal Close)> inputMapper, Func<TInput, bool?, TOutput> outputMapper, int upTrendPeriodCount = 3, int periodCount = 20, decimal longThreshold = 0.75m, decimal dojiThreshold = 0.25m, decimal threshold = 0.1m) : base(inputs, inputMapper, outputMapper)
        {
            var mappedInputs = inputs.Select(inputMapper);

            var ocs = mappedInputs.Select(c => (c.Open, c.Close));
            _upTrend = new UpTrendByTuple(mappedInputs.Select(c => (c.High, c.Low)).ToList(), upTrendPeriodCount);
            _bullishLongDay = new BullishLongDayByTuple(ocs, periodCount, longThreshold);
            _doji = new DojiByTuple(mappedInputs, dojiThreshold);
            _bearishLongDay = new BearishLongDayByTuple(ocs, periodCount, longThreshold);

            UpTrendPeriodCount = upTrendPeriodCount;
            PeriodCount = periodCount;
            LongThreshold = longThreshold;
            DojiThreshold = dojiThreshold;
            Threshold = threshold;
        }

        public int UpTrendPeriodCount { get; }
        public int PeriodCount { get; }
        public decimal LongThreshold { get; }
        public decimal DojiThreshold { get; }
        public decimal Threshold { get; }

        protected override bool? ComputeByIndexImpl(IEnumerable<(decimal Open, decimal High, decimal Low, decimal Close)> mappedInputs, int index)
        {
            if (index < 2) return null;

            Func<int, decimal> midPoint = i => (mappedInputs.ElementAt(i).Open + mappedInputs.ElementAt(i).Close) / 2;

            return (_upTrend[index - 1] ?? false) &&
                _bullishLongDay[index - 2] &&
                _doji[index - 1] &&
                (midPoint(index - 1) > mappedInputs.ElementAt(index - 2).Close) &&
                _bearishLongDay[index] && 
                (mappedInputs.ElementAt(index).Open < Math.Min(mappedInputs.ElementAt(index - 1).Open, mappedInputs.ElementAt(index - 1).Close)) && 
                Math.Abs((mappedInputs.ElementAt(index).Close - midPoint(index - 2)) / midPoint(index - 2)) < Threshold;
        }
    }

    public class EveningDojiStarByTuple : EveningDojiStar<(decimal Open, decimal High, decimal Low, decimal Close), bool?>
    {
        public EveningDojiStarByTuple(IEnumerable<(decimal Open, decimal High, decimal Low, decimal Close)> inputs, int upTrendPeriodCount = 3, int periodCount = 20, decimal longThreshold = 0.75M, decimal dojiThreshold = 0.25M, decimal threshold = 0.1M) 
            : base(inputs, i => i, (i, otm) => otm, upTrendPeriodCount, periodCount, longThreshold, dojiThreshold, threshold)
        {
        }
    }

    public class EveningDojiStar : EveningDojiStar<Candle, AnalyzableTick<bool?>>
    {
        public EveningDojiStar(IEnumerable<Candle> inputs, int upTrendPeriodCount = 3, int periodCount = 20, decimal longThreshold = 0.75M, decimal dojiThreshold = 0.25M, decimal threshold = 0.1M) 
            : base(inputs, i => (i.Open, i.High, i.Low, i.Close), (i, otm) => new AnalyzableTick<bool?>(i.DateTime, otm), upTrendPeriodCount, periodCount, longThreshold, dojiThreshold, threshold)
        {
        }
    }
}