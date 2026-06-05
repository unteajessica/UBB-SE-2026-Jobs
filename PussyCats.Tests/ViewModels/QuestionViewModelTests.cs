using Microsoft.UI.Xaml;
using Tests_and_Interviews.Models.Enums;
using Tests_and_Interviews.ViewModels;

namespace PussyCats.Tests.ViewModels
{
    public class QuestionViewModelTests
    {
        private static OptionViewModel MakeOption(int index, bool isSelected = false) =>
            new OptionViewModel
            {
                Index = index,
                IsSelected = isSelected,
            };

        private static QuestionViewModel MakeQuestion(QuestionType type) =>
            new QuestionViewModel
            {
                QuestionId = 1,
                Type = type,
            };

        [Fact]
        public void TypeLabel_ReplacesUnderscoreWithSpace()
        {
            var question = MakeQuestion(QuestionType.SINGLE_CHOICE);

            Assert.Equal("SINGLE CHOICE", question.TypeLabel);
        }

        [Fact]
        public void TrueFalseGroup_ContainsQuestionId()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            Assert.Equal("tf_1", question.TrueFalseGroup);
        }

        [Fact]
        public void TrueSelected_WhenSetToTrue_SetsFalseSelectedToFalse()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            question.FalseSelected = true;

            question.TrueSelected = true;

            Assert.False(question.FalseSelected);
        }

        [Fact]
        public void FalseSelected_WhenSetToTrue_SetsTrueSelectedToFalse()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            question.TrueSelected = true;

            question.FalseSelected = true;

            Assert.False(question.TrueSelected);
        }

        [Fact]
        public void TrueSelected_WhenSetToFalse_DoesNotClearFalseSelected()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            question.FalseSelected = true;

            question.TrueSelected = false;

            Assert.True(question.FalseSelected);
        }

        #region RaisesPropertyChanged

        [Fact]
        public void TrueSelected_WhenChanged_RaisesPropertyChangedForTrueSelected()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            var raisedProperties = new List<string>();

            question.PropertyChanged += (sender, eventArgs) =>
                raisedProperties.Add(eventArgs.PropertyName!);

            question.TrueSelected = true;

            Assert.Contains("TrueSelected", raisedProperties);
        }

        [Fact]
        public void TrueSelected_WhenChanged_RaisesPropertyChangedForFalseSelected()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            var raisedProperties = new List<string>();

            question.PropertyChanged += (sender, eventArgs) =>
                raisedProperties.Add(eventArgs.PropertyName!);

            question.TrueSelected = true;

            Assert.Contains("FalseSelected", raisedProperties);
        }

        [Fact]
        public void FalseSelected_WhenChanged_RaisesPropertyChangedForFalseSelected()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            var raisedProperties = new List<string>();

            question.PropertyChanged += (sender, eventArgs) =>
                raisedProperties.Add(eventArgs.PropertyName!);

            question.FalseSelected = true;

            Assert.Contains("FalseSelected", raisedProperties);
        }

        [Fact]
        public void FalseSelected_WhenChanged_RaisesPropertyChangedForTrueSelected()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            var raisedProperties = new List<string>();

            question.PropertyChanged += (sender, eventArgs) =>
                raisedProperties.Add(eventArgs.PropertyName!);

            question.FalseSelected = true;

            Assert.Contains("TrueSelected", raisedProperties);
        }

        [Fact]
        public void TextAnswer_WhenChanged_RaisesPropertyChanged()
        {
            var question = MakeQuestion(QuestionType.TEXT);

            string? raisedProperty = null;

            question.PropertyChanged += (sender, eventArgs) =>
                raisedProperty = eventArgs.PropertyName;

            question.TextAnswer = "answer";

            Assert.Equal("TextAnswer", raisedProperty);
        }

        [Fact]
        public void TrueSelected_WhenChanged_InvokesOnAnswerChanged()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            var invoked = false;

            question.OnAnswerChanged = () => invoked = true;

            question.TrueSelected = true;

            Assert.True(invoked);
        }

        [Fact]
        public void FalseSelected_WhenChanged_InvokesOnAnswerChanged()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            var invoked = false;

            question.OnAnswerChanged = () => invoked = true;

            question.FalseSelected = true;

            Assert.True(invoked);
        }

        [Fact]
        public void TextAnswer_WhenChanged_InvokesOnAnswerChanged()
        {
            var question = MakeQuestion(QuestionType.TEXT);

            var invoked = false;

            question.OnAnswerChanged = () => invoked = true;

            question.TextAnswer = "answer";

            Assert.True(invoked);
        }

        #endregion

        #region GetAnswerValue

        [Fact]
        public void GetAnswerValue_SingleChoice_WhenOptionSelected_ReturnsIndex()
        {
            var question = MakeQuestion(QuestionType.SINGLE_CHOICE);

            question.Options.Add(MakeOption(index: 0, isSelected: false));
            question.Options.Add(MakeOption(index: 1, isSelected: true));

            Assert.Equal("1", question.GetAnswerValue());
        }

        [Fact]
        public void GetAnswerValue_SingleChoice_WhenNoOptionSelected_ReturnsEmpty()
        {
            var question = MakeQuestion(QuestionType.SINGLE_CHOICE);

            question.Options.Add(MakeOption(index: 0, isSelected: false));

            Assert.Equal(string.Empty, question.GetAnswerValue());
        }

        [Fact]
        public void GetAnswerValue_MultipleChoice_WhenOptionsSelected_ReturnsIndexArray()
        {
            var question = MakeQuestion(QuestionType.MULTIPLE_CHOICE);

            question.Options.Add(MakeOption(index: 0, isSelected: true));
            question.Options.Add(MakeOption(index: 1, isSelected: false));
            question.Options.Add(MakeOption(index: 2, isSelected: true));

            Assert.Equal("[0,2]", question.GetAnswerValue());
        }

        [Fact]
        public void GetAnswerValue_MultipleChoice_WhenNoOptionsSelected_ReturnsEmptyArray()
        {
            var question = MakeQuestion(QuestionType.MULTIPLE_CHOICE);

            question.Options.Add(MakeOption(index: 0, isSelected: false));

            Assert.Equal("[]", question.GetAnswerValue());
        }

        [Fact]
        public void GetAnswerValue_TrueFalse_WhenTrueSelected_ReturnsTrue()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            question.TrueSelected = true;

            Assert.Equal("true", question.GetAnswerValue());
        }

        [Fact]
        public void GetAnswerValue_TrueFalse_WhenFalseSelected_ReturnsFalse()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            question.FalseSelected = true;

            Assert.Equal("false", question.GetAnswerValue());
        }

        [Fact]
        public void GetAnswerValue_TrueFalse_WhenNeitherSelected_ReturnsEmpty()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            Assert.Equal(string.Empty, question.GetAnswerValue());
        }

        [Fact]
        public void GetAnswerValue_Text_ReturnsTrimmedAnswer()
        {
            var question = MakeQuestion(QuestionType.TEXT);

            question.TextAnswer = "  hello  ";

            Assert.Equal("hello", question.GetAnswerValue());
        }

        [Fact]
        public void GetAnswerValue_UnknownType_ReturnsEmpty()
        {
            var question = MakeQuestion((QuestionType)99);

            Assert.Equal(string.Empty, question.GetAnswerValue());
        }

        #endregion

        #region IsAnswered

        [Fact]
        public void IsAnswered_SingleChoice_WhenOptionSelected_ReturnsTrue()
        {
            var question = MakeQuestion(QuestionType.SINGLE_CHOICE);

            question.Options.Add(MakeOption(index: 0, isSelected: true));

            Assert.True(question.IsAnswered());
        }

        [Fact]
        public void IsAnswered_SingleChoice_WhenNoOptionSelected_ReturnsFalse()
        {
            var question = MakeQuestion(QuestionType.SINGLE_CHOICE);

            question.Options.Add(MakeOption(index: 0, isSelected: false));

            Assert.False(question.IsAnswered());
        }

        [Fact]
        public void IsAnswered_MultipleChoice_WhenOptionSelected_ReturnsTrue()
        {
            var question = MakeQuestion(QuestionType.MULTIPLE_CHOICE);

            question.Options.Add(MakeOption(index: 0, isSelected: true));

            Assert.True(question.IsAnswered());
        }

        [Fact]
        public void IsAnswered_MultipleChoice_WhenNoOptionSelected_ReturnsFalse()
        {
            var question = MakeQuestion(QuestionType.MULTIPLE_CHOICE);

            question.Options.Add(MakeOption(index: 0, isSelected: false));

            Assert.False(question.IsAnswered());
        }

        [Fact]
        public void IsAnswered_TrueFalse_WhenTrueSelected_ReturnsTrue()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            question.TrueSelected = true;

            Assert.True(question.IsAnswered());
        }

        [Fact]
        public void IsAnswered_TrueFalse_WhenFalseSelected_ReturnsTrue()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            question.FalseSelected = true;

            Assert.True(question.IsAnswered());
        }

        [Fact]
        public void IsAnswered_TrueFalse_WhenNeitherSelected_ReturnsFalse()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            Assert.False(question.IsAnswered());
        }

        [Fact]
        public void IsAnswered_Text_WhenTextProvided_ReturnsTrue()
        {
            var question = MakeQuestion(QuestionType.TEXT);

            question.TextAnswer = "answer";

            Assert.True(question.IsAnswered());
        }

        [Fact]
        public void IsAnswered_Text_WhenTextIsWhitespace_ReturnsFalse()
        {
            var question = MakeQuestion(QuestionType.TEXT);

            question.TextAnswer = "   ";

            Assert.False(question.IsAnswered());
        }

        [Fact]
        public void IsAnswered_UnknownType_ReturnsFalse()
        {
            var question = MakeQuestion((QuestionType)99);

            Assert.False(question.IsAnswered());
        }

        #endregion

        #region Visibility

        [Fact]
        public void IsSingleChoice_TypeMatch_AssignsVisibilityVisible()
        {
            var question = MakeQuestion(QuestionType.SINGLE_CHOICE);

            Assert.Equal(Visibility.Visible, question.IsSingleChoice);
        }

        [Fact]
        public void IsSingleChoice_TypeMismatch_AssignsVisibilityCollapsed()
        {
            var question = MakeQuestion(QuestionType.INTERVIEW);

            Assert.Equal(Visibility.Collapsed, question.IsSingleChoice);
        }

        [Fact]
        public void IsMultipleChoice_TypeMatch_AssignsVisibilityVisible()
        {
            var question = MakeQuestion(QuestionType.MULTIPLE_CHOICE);

            Assert.Equal(Visibility.Visible, question.IsMultipleChoice);
        }

        [Fact]
        public void IsMultipleChoice_TypeMismatch_AssignsVisibilityCollapsed()
        {
            var question = MakeQuestion(QuestionType.INTERVIEW);

            Assert.Equal(Visibility.Collapsed, question.IsMultipleChoice);
        }

        [Fact]
        public void IsTrueFalse_TypeMatch_AssignsVisibilityVisible()
        {
            var question = MakeQuestion(QuestionType.TRUE_FALSE);

            Assert.Equal(Visibility.Visible, question.IsTrueFalse);
        }

        [Fact]
        public void IsTrueFalse_TypeMismatch_AssignsVisibilityCollapsed()
        {
            var question = MakeQuestion(QuestionType.INTERVIEW);

            Assert.Equal(Visibility.Collapsed, question.IsTrueFalse);
        }

        [Fact]
        public void IsText_TypeMatch_AssignsVisibilityVisible()
        {
            var question = MakeQuestion(QuestionType.TEXT);

            Assert.Equal(Visibility.Visible, question.IsText);
        }

        [Fact]
        public void IsText_TypeMismatch_AssignsVisibilityCollapsed()
        {
            var question = MakeQuestion(QuestionType.INTERVIEW);

            Assert.Equal(Visibility.Collapsed, question.IsText);
        }

        #endregion

    }
}