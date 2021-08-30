import java.lang.reflect.Method;

class TestClass {
    private int value;
    public int getValue() { return value; }
    public void setValue(int valueIn) { this.value = valueIn; }
}

public class Main {
    public static void main(String[] args) {
        var testClass = new TestClass();

        for (var field: testClass.getClass().getDeclaredFields()) {
            System.out.printf("name:%s, type:%s \n", field.getName(), field.getType().getCanonicalName());
        }

        for (var method : testClass.getClass().getDeclaredMethods()) {
            System.out.printf("name:%s, return type:%s  \n", method.getName(), method.getReturnType().getCanonicalName());
        }        
    }
}