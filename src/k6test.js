import http from 'k6/http';
import { Counter } from 'k6/metrics';
import { check, sleep } from 'k6';

const allowedRequests = new Counter('rate_limit_allowed');
const blockedRequests = new Counter('rate_limit_blocked');

export const options = {
  stages: [
    { duration: '10s', target: 50 },
    { duration: '30s', target: 50 },
    { duration: '10s', target: 0 },
  ],
};

export default function () {
  const res = http.get('http://localhost:5068/');

  if (res.status === 200) {
    allowedRequests.add(1);
  } else if (res.status === 429) {
    blockedRequests.add(1);
  }

  check(res, {
    'is not a 500 error': (r) => r.status < 500,
  });

  sleep(0.01);
}
