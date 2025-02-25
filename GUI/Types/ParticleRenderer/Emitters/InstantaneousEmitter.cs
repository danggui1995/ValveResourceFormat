using System;
using ValveResourceFormat.Serialization;

namespace GUI.Types.ParticleRenderer.Emitters
{
    class InstantaneousEmitter : IParticleEmitter
    {
        public bool IsFinished { get; private set; }

        private Action particleEmitCallback;

        private readonly INumberProvider emitCount;
        private readonly INumberProvider startTime;

        private float time;

        public InstantaneousEmitter(IKeyValueCollection keyValues)
        {
            emitCount = keyValues.GetNumberProvider("m_nParticlesToEmit");
            startTime = keyValues.GetNumberProvider("m_flStartTime");
        }

        public void Start(Action particleEmitCallback)
        {
            this.particleEmitCallback = particleEmitCallback;

            IsFinished = false;

            time = 0;
        }

        public void Stop()
        {
        }

        public void Update(float frameTime)
        {
            time += frameTime;

            if (!IsFinished && time >= startTime.NextNumber())
            {
                var numToEmit = emitCount.NextInt(); // Get value from number provider
                for (var i = 0; i < numToEmit; i++)
                {
                    particleEmitCallback();
                }

                IsFinished = true;
            }
        }
    }
}
