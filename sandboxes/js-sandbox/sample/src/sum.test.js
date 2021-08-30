import { expect } from "chai";
import sum from "./sum";
import FakeTimers from "@sinonjs/fake-timers";

describe("sum:", () => {
  it("should sum 2 and 2", () => {
    expect(sum(2, 2)).to.equal(4);
  });

  it('should sum "2" and "2"', () => {
    expect(sum("2", "2")).to.equal("22");
  });

  it("should work async", async () => {
    const result = await sum(2, 2);
    expect(result).to.equal(4);
  });

  it("should work with timers", () => {
    const clock = FakeTimers.createClock();
    let result = -1;
    clock.setTimeout(() => {
      result = sum(2, 2);
    }, 1000);
    expect(result).to.equal(-1);
    clock.tick(500);
    expect(result).to.equal(-1);
    clock.tick(500);
    expect(result).to.equal(4);
  });
});
