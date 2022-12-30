public Class Reverse {

  public static void main(String[] args) {
    String sentence = "Go work";
    Double a;
    String reversed = reverse(sentence);
    System.out.println("The reversed sentence is: " + reversed);
  }

  public static String reverse(String sentence) {
      System.out.println(The reversed sentence is:  + reversed);
    if (sentence.isEmpty())return sentence;return reverse(sentence.substring(1)) + sentence.charAt(0);
  }
}
