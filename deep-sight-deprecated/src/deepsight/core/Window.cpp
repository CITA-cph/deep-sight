#include "Window.h"

namespace DeepSight
{
	static const struct
	{
	    float x, y;
	    float r, g, b;
	} vertices[3] =
	{
	    { -0.6f, -0.4f, 1.f, 0.f, 0.f },
	    {  0.6f, -0.4f, 0.f, 1.f, 0.f },
	    {   0.f,  0.6f, 0.f, 0.f, 1.f }
	};

	static const char* vertex_shader_text =
	"#version 110\n"
	"uniform mat4 MVP;\n"
	"attribute vec3 vCol;\n"
	"attribute vec2 vPos;\n"
	"varying vec3 color;\n"
	"void main()\n"
	"{\n"
	"    gl_Position = MVP * vec4(vPos, 0.0, 1.0);\n"
	"    color = vCol;\n"
	"}\n";
	 
	static const char* fragment_shader_text =
	"#version 110\n"
	"varying vec3 color;\n"
	"void main()\n"
	"{\n"
	"    gl_FragColor = vec4(color, 1.0);\n"
	"}\n";

	int TestOpenGLWindow()
	{
	    GLFWwindow* window;
        GLuint vertex_buffer, vertex_shader, fragment_shader, program;
    	GLint mvp_location, vpos_location, vcol_location;

	    /* Initialize the library */
	    if (!glfwInit())
	        return -1;

	    /* Create a windowed mode window and its OpenGL context */
	    window = glfwCreateWindow(640, 480, "Hello World", NULL, NULL);
	    if (!window)
	    {
	        glfwTerminate();
	        return -1;
	    }

	    /* Make the window's context current */
	    glfwMakeContextCurrent(window);

        gladLoadGLLoader((GLADloadproc)glfwGetProcAddress);
	    glfwSwapInterval(1);
	 
	    // NOTE: OpenGL error checks have been omitted for brevity
	 
	    glGenBuffers(1, &vertex_buffer);
	    glBindBuffer(GL_ARRAY_BUFFER, vertex_buffer);
	    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
	 
	    vertex_shader = glCreateShader(GL_VERTEX_SHADER);
	    glShaderSource(vertex_shader, 1, &vertex_shader_text, NULL);
	    glCompileShader(vertex_shader);
	 
	    fragment_shader = glCreateShader(GL_FRAGMENT_SHADER);
	    glShaderSource(fragment_shader, 1, &fragment_shader_text, NULL);
	    glCompileShader(fragment_shader);
	 
	    program = glCreateProgram();
	    glAttachShader(program, vertex_shader);
	    glAttachShader(program, fragment_shader);
	    glLinkProgram(program);
	 
	    mvp_location = glGetUniformLocation(program, "MVP");
	    vpos_location = glGetAttribLocation(program, "vPos");
	    vcol_location = glGetAttribLocation(program, "vCol");
	 
	    glEnableVertexAttribArray(vpos_location);
	    glVertexAttribPointer(vpos_location, 2, GL_FLOAT, GL_FALSE,
	                          sizeof(vertices[0]), (void*) 0);
	    glEnableVertexAttribArray(vcol_location);
	    glVertexAttribPointer(vcol_location, 3, GL_FLOAT, GL_FALSE,
	                          sizeof(vertices[0]), (void*) (sizeof(float) * 2));


	    /* Loop until the user closes the window */
	    while (!glfwWindowShouldClose(window))
	    {
	        /* Render here */
	        glClear(GL_COLOR_BUFFER_BIT);

	        float ratio;
	        int width, height;
	        mat4x4 m, p, mvp;
	 
	        glfwGetFramebufferSize(window, &width, &height);
	        ratio = width / (float) height;
	 
	        glViewport(0, 0, width, height);
	        glClear(GL_COLOR_BUFFER_BIT);
	 
	        mat4x4_identity(m);
	        mat4x4_rotate_Z(m, m, (float) glfwGetTime());
	        mat4x4_ortho(p, -ratio, ratio, -1.f, 1.f, 1.f, -1.f);
	        mat4x4_mul(mvp, p, m);
	 
	        glUseProgram(program);
	        glUniformMatrix4fv(mvp_location, 1, GL_FALSE, (const GLfloat*) mvp);
	        glDrawArrays(GL_TRIANGLES, 0, 3);

	        /* Swap front and back buffers */
	        glfwSwapBuffers(window);

	        /* Poll for and process events */
	        glfwPollEvents();
	    }

	    glfwTerminate();
	    return 0;
	}
}