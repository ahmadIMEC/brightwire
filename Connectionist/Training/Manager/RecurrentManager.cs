﻿using Icbld.BrightWire.Connectionist.Training.Helper;
using Icbld.BrightWire.Models;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.Connectionist.Training.Manager
{
    internal class RecurrentManager : RecurrentManagerBase, IRecurrentTrainingManager
    {
        readonly INeuralNetworkRecurrentBatchTrainer _trainer;
        readonly string _dataFile;
        double _bestScore;
        RecurrentNetwork _bestOutput;
        float[] _memory;

        public RecurrentManager(INeuralNetworkRecurrentBatchTrainer trainer, string dataFile, IReadOnlyList<Tuple<float[], float[]>[]> testData, IErrorMetric errorMetric, int memorySize)
            : base(testData, errorMetric)
        {
            _trainer = trainer;
            _dataFile = dataFile;
            _memory = _Load(_trainer, _dataFile, memorySize);
        }

        public INeuralNetworkRecurrentBatchTrainer Trainer { get { return _trainer; } }

        public float[] Memory
        {
            get
            {
                return _memory;
            }
        }

        public void Train(IReadOnlyList<Tuple<float[], float[]>[]> trainingData, int numEpochs, ITrainingContext context, IRecurrentTrainingContext recurrentContext = null)
        {
            if (recurrentContext == null)
                recurrentContext = new RecurrentContext(_trainer.LinearAlgebraProvider, context);

            _bestScore = _GetScore(_testData, _trainer, _memory, recurrentContext);
            Console.WriteLine(_errorMetric.DisplayAsPercentage ? "Initial score: {0:P}" : "Initial score: {0}", _bestScore);

            _bestOutput = null;
            recurrentContext.TrainingContext.RecurrentEpochComplete += OnEpochComplete;
            _memory = _trainer.Train(trainingData, _memory, numEpochs, recurrentContext);
            recurrentContext.TrainingContext.RecurrentEpochComplete -= OnEpochComplete;

            if (_bestOutput != null) {
                using (var stream = new FileStream(_dataFile, FileMode.Create, FileAccess.Write))
                    Serializer.Serialize(stream, _bestOutput);

                // ensure best values are current
                _trainer.NetworkInfo = _bestOutput;
                _memory = _bestOutput.Memory.Data;
            }
        }

        private void OnEpochComplete(ITrainingContext context, IRecurrentTrainingContext recurrentContext)
        {
            _AfterEpoch(context, _memory, _testData, _trainer, recurrentContext, ref _bestScore, ref _bestOutput);
        }
    }
}
