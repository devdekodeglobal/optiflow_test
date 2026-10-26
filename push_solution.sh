set -e
git init
git branch -m main
git add .
git commit -m "feat(allocation): add .NET 10 OptiFlow allocation engine with DDD, Template Method pattern, guardrails and unit tests"
git remote add origin git@github.com:devdekodeglobal/optiflow_test.git
git push -u origin main
