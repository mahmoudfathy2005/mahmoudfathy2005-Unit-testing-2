# Testing Notes

## 1. Test Results
- Total tests: 26
- Passed: 26
- Failed: 0
- Skipped: 0

## 2. Line Coverage
- Overall line coverage: 90.5%
- Domain: 100%
- Application: 95.8%
- Branch coverage: 83.3%

Both Domain and Application exceed the required 80% line coverage.

## 3. Testing Tools

- **Moq:** Used to mock IInventoryService, IOrderRepository, and INotificationService, and verify their interactions.
- **AutoFixture:** Used to generate test orders with customized valid quantities and prices.
- **Bogus:** Used to generate realistic customer data with a fixed seed to ensure deterministic tests.

## 4. Discount Boundary

Yes, according to the implemented business rule, orders below 1,000 EGP receive no discount, while orders with a subtotal of 1,000 EGP or more receive a 10% discount capped at 100 EGP.

## 5. Bugs Found

None.