/**
 * ============================================================================
 * STRESS TEST
 * ============================================================================
 *
 * Кожен рівень навантаження запускається окремим послідовним сценарієм із
 * власним тегом phase. Завдяки цьому k6 показує окремі метрики для 100, 200,
 * 400, 600, 800, 1000, 1300, 1500, 1800 VU і recovery-фази, а не лише
 * загальний результат тесту.
 */

import crudIteration from './crud-load-test.js';
import { DEFAULT_THRESHOLDS } from '../config/endpoints.js';

const warmup = 30;
const hold = 60;
const cooldown = 30;

function phaseStages(target) {
  return [
    { duration: `${warmup}s`, target },
    { duration: `${hold}s`, target },
    { duration: '10s', target: 0 },
  ];
}

function phaseScenario(name, startTime, target) {
  return {
    executor: 'ramping-vus',
    exec: 'crudScenario',
    startTime,
    stages: phaseStages(target),
    gracefulRampDown: '10s',
    gracefulStop: '10s',
    tags: { phase: name },
  };
}

function phaseThresholds(phase) {
  const selector = (metric, extraTags = '') =>
    `${metric}{phase:${phase}${extraTags ? `,${extraTags}` : ''}}`;

  return {
    [selector('api_errors')]: ['rate==0'],
    [selector('checks')]: ['rate>0.95'],
    [selector('http_req_failed')]: ['rate<0.01'],
    [selector('http_req_duration')]: ['p(99)<2000'],
    [selector('http_req_duration', 'type:read')]: [
      'p(95)<500',
      'p(99)<1000',
    ],
    [selector('http_req_duration', 'type:write')]: [
      'p(95)<1000',
      'p(99)<2000',
    ],
    [selector('optimistic_lock_conflicts')]: ['rate>0'],
  };
}

// Each phase lasts 100 seconds: 30s ramp-up + 60s hold + 10s ramp-down.
export const options = {
  scenarios: {
    stress_100: phaseScenario('stress-100-vus', '0s', 100),
    stress_200: phaseScenario('stress-200-vus', '100s', 200),
    stress_400: phaseScenario('stress-400-vus', '200s', 400),
    stress_600: phaseScenario('stress-600-vus', '300s', 600),
    stress_800: phaseScenario('stress-800-vus', '400s', 800),
    stress_1000: phaseScenario('stress-1000-vus', '500s', 1000),
    stress_1300: phaseScenario('stress-1300-vus', '600s', 1300),
    stress_1500: phaseScenario('stress-1500-vus', '700s', 1500),
    stress_1800: phaseScenario('stress-1800-vus', '800s', 1800),
    recovery: {
      executor: 'constant-vus',
      exec: 'crudScenario',
      startTime: '910s',
      vus: 10,
      duration: '60s',
      gracefulStop: '10s',
      tags: { phase: 'recovery-10-vus' },
    },
  },

  thresholds: {
    ...DEFAULT_THRESHOLDS,
    ...phaseThresholds('stress-100-vus'),
    ...phaseThresholds('stress-200-vus'),
    ...phaseThresholds('stress-400-vus'),
    ...phaseThresholds('stress-600-vus'),
    ...phaseThresholds('stress-800-vus'),
    ...phaseThresholds('stress-1000-vus'),
    ...phaseThresholds('stress-1300-vus'),
    ...phaseThresholds('stress-1500-vus'),
    ...phaseThresholds('stress-1800-vus'),
    ...phaseThresholds('recovery-10-vus'),
  },

  userAgent: 'K6-StressTest/2.0',
};

export function crudScenario() {
  crudIteration();
}

export function setup() {
  console.log('='.repeat(80));
  console.log('Starting tagged CRUD stress test');
  console.log('Phases: 100 → 200 → 400 → 600 → 800 → 1000 → 1300 → 1500 → 1800 VUs');
  console.log('Recovery phase: 10 VUs after the load phases');
  console.log('Each phase: 30s ramp-up + 60s hold + 10s ramp-down');
  console.log('Metrics are grouped by the phase tag');
  console.log('='.repeat(80));
}

export function teardown() {
  console.log('='.repeat(80));
  console.log('Tagged CRUD stress test completed');
  console.log('Compare phase-specific p95, p99, errors and checks');
  console.log('Recovery is successful only if recovery-10-vus returns to normal');
  console.log('='.repeat(80));
}
