using System;

namespace ReferenceTests
{
    public class ReferenceTestBaseWithTestModule : ReferenceTestBase
    {

        public override void SetUp()
        {
            base.SetUp();
            ImportTestCmdlets();
        }

        public override void TearDown()
        {
            base.TearDown();
            CleanImports();
        }

    }
}

