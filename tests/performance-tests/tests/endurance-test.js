/**
 * ============================================================================
 * ENDURANCE TEST
 * ============================================================================
 *
 * Перевіряє стабільність API під тривалим навантаженням протягом 30 хвилин:
 * 1. 5 хвилин — поступовий вихід на 1500 VU.
 * 2. 20 хвилин — стабільне навантаження 1500 VU.
 * 3. 5 хвилин — поступове зниження до 0 VU.
 *
 * Повний CRUD-цикл імпортується з crud-load-test.js. JSON-результати містять
 * часові ряди, за якими можна порівняти latency та error rate на початку,
 * в середині й наприкінці steady-state фази.
 */

import crudIteration from './crud-load-test.js';
import { DEFAULT_THRESHOLDS } from '../config/endpoints.js';

export const options = {
  stages: [
    { duration: '5m', target: 1500 },
    { duration: '20m', target: 1500 },
    { duration: '5m', target: 0 },
  ],

  thresholds: {
    ...DEFAULT_THRESHOLDS,
    api_errors: ['rate==0'],
    checks: ['rate>0.95'],
    optimistic_lock_conflicts: ['rate>0'],
    'http_req_duration{type:read}': [
      'p(95)<500',
      'p(99)<1000',
    ],
    'http_req_duration{type:write}': [
      'p(95)<1000',
      'p(99)<2000',
    ],
  },

  noConnectionReuse: false,
  userAgent: 'K6-EnduranceTest/1.0',
};

export default function () {
  crudIteration();
}

export function setup() {
  console.log('='.repeat(80));
  console.log('Starting 30-minute CRUD endurance test');
  console.log('Stages: 5m ramp-up → 20m at 1500 VUs → 5m ramp-down');
  console.log('Analyze JSON time series for latency and error-rate changes over time');
  console.log('='.repeat(80));
}

export function teardown() {
  console.log('='.repeat(80));
  console.log('30-minute CRUD endurance test completed');
  console.log('Compare early, middle and late steady-state metrics');
  console.log('Look for increasing latency, errors or interrupted iterations');
  console.log('='.repeat(80));
}
