﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.ErrorMetric
{
    public class OneHotEncoding : IErrorMetric
    {
        public float Compute(IIndexableVector output, IIndexableVector expectedOutput)
        {
            return output.MaximumIndex() == expectedOutput?.MaximumIndex() ? 1f : 0f;
        }

        public IMatrix CalculateDelta(IMatrix output, IMatrix targetOutput)
        {
            return targetOutput.Subtract(output);
        }
    }
}
